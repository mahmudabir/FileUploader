import { Component, OnInit, ViewChild, ElementRef } from '@angular/core';
import { HttpClient, HttpEventType, HttpResponse } from '@angular/common/http';
import { CommonModule } from '@angular/common';

@Component({
  selector: 'app-video-player',
  templateUrl: `./video-player.component.html`,
  standalone: true,
  imports: [
    CommonModule,
  ],
})
export class VideoPlayerComponent implements OnInit {
  @ViewChild('videoPlayer') videoPlayer!: ElementRef<HTMLVideoElement>;

  private loadFullVideo = true;

  private apiUrl = 'https://localhost:7001/api/video';
  private fileName = 'Wednesday S01E02  1080p NF WEBRip x265 HEVC MSubs [Dual Audio][Hindi 5.1+English 5.1] -OlaM.mkv';//'Avatar The Last Airbender S01E01.mp4';
  private chunkSize = 10 * 1024 * 1024; // 10 MB
  private mediaSource: MediaSource;
  private sourceBuffer: SourceBuffer | null = null;
  private queue: ArrayBuffer[] = [];
  private currentChunk = 0;
  totalSize = 0;
  loadedSize = 0;

  isLoading = false;
  isCompleted = false;
  private shouldEndStream = false;

  constructor(private http: HttpClient) {
    this.mediaSource = new MediaSource();
  }

  ngOnInit() {
    // Initialization that doesn't depend on the view
  }

  ngAfterViewInit() {
    this.initializeVideo();
  }

  private initializeVideo() {
    const video = this.videoPlayer.nativeElement;
    video.src = URL.createObjectURL(this.mediaSource);

    this.mediaSource.addEventListener('sourceopen', () => {
      this.sourceBuffer = this.mediaSource.addSourceBuffer('video/mp4; codecs="avc1.42E01E, mp4a.40.2"');
      this.loadNextChunk();

      this.sourceBuffer.addEventListener('updateend', () => {
        this.appendNextChunk();
        if (this.shouldEndStream) {
          this.endOfStream();
        }
      });
    });

    video.addEventListener('timeupdate', () => {
      if (!this.isCompleted && video.currentTime > (this.currentChunk + 1) * this.chunkSize / video.duration && !this.isLoading) {
        this.loadNextChunk();
      }
    });

    video.addEventListener('seeking', () => {
      if (this.isCompleted) return;
      const seekChunk = Math.floor(video.currentTime * video.duration / this.chunkSize);
      if (seekChunk !== this.currentChunk) {
        this.currentChunk = seekChunk;
        this.queue = [];
        this.loadNextChunk();
      }
    });
  }

  private loadNextChunk() {
    if (this.isLoading || this.isCompleted) return;
    this.isLoading = true;
    const start = this.currentChunk * this.chunkSize;
    this.http.get(`${this.apiUrl}/${this.fileName}?start=${start}&getFullSize=${this.loadFullVideo}`, {
      responseType: 'arraybuffer',
      observe: 'response'
    }).subscribe(
      (response: HttpResponse<ArrayBuffer>) => {
        if (response.body) {
          this.queue.push(response.body);
          this.loadedSize += response.body.byteLength;
          this.currentChunk++;
        }

        const totalSizeHeader = response.headers.get('X-Total-File-Size');
        if (totalSizeHeader) {
          this.totalSize = parseInt(totalSizeHeader, 10);
        }

        this.isLoading = false;
        this.appendNextChunk();

        if (this.loadedSize >= this.totalSize) {
          this.isCompleted = true;
          this.shouldEndStream = true;

          const video = this.videoPlayer.nativeElement;
          video.src = URL.createObjectURL( new Blob([response.body], { type: 'application/octet-stream' }));
        }
      },
      error => {
        console.error('Error loading video chunk:', error);
        this.isLoading = false;
      }
    );
  }

  private appendNextChunk() {
    if (!this.sourceBuffer || this.sourceBuffer.updating || this.queue.length === 0) {
      return;
    }

    const chunk = this.queue.shift();
    if (chunk) {
      this.sourceBuffer.appendBuffer(chunk);
    } else if (this.shouldEndStream) {
      this.endOfStream();
    } else if (this.queue.length === 0 && !this.isLoading && !this.isCompleted) {
      this.loadNextChunk();
    }
  }

  private endOfStream() {
    if (this.mediaSource.readyState === 'open' && this.sourceBuffer && !this.sourceBuffer.updating) {
      this.mediaSource.endOfStream();
      this.shouldEndStream = false;
    }
  }
}