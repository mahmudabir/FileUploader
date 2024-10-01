import { CommonModule } from '@angular/common';
import { HttpClient } from '@angular/common/http';
import { Component, ElementRef, OnInit, ViewChild } from '@angular/core';
import { ActivatedRoute, Router } from '@angular/router';
import { VideoPlayerComponent } from "../video-player/video-player.component";

@Component({
  selector: 'app-video-stream',
  templateUrl: `./video-stream.component.html`,
  styleUrl: './video-stream.component.css',
  standalone: true,
  imports: [
    CommonModule,
    VideoPlayerComponent
],
})
export class VideoStreamComponent implements OnInit {
  baseUrl = 'https://localhost:7001/api';
  videoUrl: string;
  files: string[] = [];
  showPlayer = false;

  constructor(private http: HttpClient, private route: ActivatedRoute, private router: Router) {
  }

  ngOnInit() {
    this.getAllVideos();
  }

  ngAfterViewInit() {
    // setTimeout(() => {
    //   this.route.queryParams.subscribe(params => {
    //     this.fileName = params['fileName'];

    //     if (this.fileName) {
    //       this.videoUrl = `${this.baseUrl}/Streaming/master.m3u8?file=${this.fileName}`;
    //       this.showPlayer = true;
    //     }

    //   });
    // }, 1000);
  }

  getAllVideos() {
    this.http.get<string[]>(`${this.baseUrl}/files`)
      .subscribe(res => this.files = res);
  }

  play(file: string) {

    this.showPlayer = false;

    this.videoUrl = `${this.baseUrl}/Streaming/master.m3u8?file=${file}`;

    setTimeout(() => {
      this.showPlayer = true;
    }, 100);

    // window.location.href = '/video?fileName=' + file;
  }

}