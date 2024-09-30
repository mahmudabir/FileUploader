import { CommonModule } from '@angular/common';
import { HttpClient } from '@angular/common/http';
import { Component, ElementRef, OnInit, ViewChild } from '@angular/core';
import { BrowserModule } from '@angular/platform-browser';
import { ActivatedRoute, Router } from '@angular/router';
import fluidPlayer from 'fluid-player';

@Component({
  selector: 'app-video-player',
  templateUrl: `./video-player.component.html`,
  styleUrl: './video-player.component.css',
  standalone: true,
  imports: [
    CommonModule
  ],
})
export class VideoPlayerComponent implements OnInit {
  @ViewChild('videoPlayer') videoPlayer!: ElementRef<HTMLVideoElement>;

  baseUrl = 'https://localhost:7001/api';
  videoUrl: string;
  masterPlaylist: any;


  files: string[] = [];

  fileName: string = "";

  player: FluidPlayerInstance;


  showPlayer = false;


  constructor(private http: HttpClient, private route: ActivatedRoute, private router: Router) {
  }

  ngOnInit() {

    this.getAllVideos();
  }

  ngAfterViewInit() {
    setTimeout(() => {
      this.route.queryParams.subscribe(params => {
        this.fileName = params['fileName'];

        if (this.fileName) {
          this.videoUrl = `${this.baseUrl}/Streaming/master.m3u8?file=${this.fileName}`;
          console.log(this.fileName);

          this.http.get(`${this.baseUrl}/Streaming/master.m3u8`, { params: { file: this.fileName }, responseType: 'blob', })
            .subscribe(res => {
              this.masterPlaylist = URL.createObjectURL(res);
              this.showPlayer = true;

              setTimeout(() => {
                this.initializeFluidPlayer();
              }, 500);
            });
        }

      });
    }, 1000);
  }

  getAllVideos() {
    this.http.get<string[]>(`${this.baseUrl}/files`)
      .subscribe(res => this.files = res);
  }

  play(file: string) {

    // this.showPlayer = false;

    this.videoUrl = `${this.baseUrl}/Streaming/master.m3u8?file=${file}`;

    window.location.href = '/video?fileName=' + file;

    // this.showPlayer = true;
    // setTimeout(() => {
    //   this.initializeFluidPlayer();
    // }, 1000);

    // // this.player.destroy();

    // // this.initializeFluidPlayer();

    // // this.http.get(`${this.videoUrl}`, { params: { file }, responseType: 'blob', })
    // //   .subscribe(res => this.masterPlaylist = URL.createObjectURL(res));

    // console.log(this.videoUrl);

    // if (this.videoPlayer) {
    //   this.videoPlayer.nativeElement.pause(); // Stop the current video
    //   this.videoPlayer.nativeElement.src = this.videoUrl;
    //   // const sourceElement = this.videoPlayer.nativeElement.getElementsByTagName('source')[0];

    //   // Update the source URL dynamically
    //   // sourceElement.src = this.videoUrl;

    //   // Load the new source file
    //   this.videoPlayer.nativeElement.load();
    //   this.videoPlayer.nativeElement.play(); // Optionally play the video after loading
    // }


    // this.initializeFluidPlayer();
    // this.player.play();
  }

  initializeFluidPlayer() {
    this.player = fluidPlayer('videoElement', {
      layoutControls: {
        persistentSettings: {
          volume: true, // Default true
          quality: true, // Default true
          speed: true, // Default true
          theatre: true // Default true
        },
        controlBar: {
          autoHideTimeout: 3,
          animated: true,
          autoHide: true
        },
        htmlOnPauseBlock: {
          html: null,
          height: null,
          width: null
        },
        autoPlay: true,
        mute: true,
        allowTheatre: true,
        playPauseAnimation: true,
        playbackRateEnabled: true,
        allowDownload: true,
        playButtonShowing: true,
        fillToContainer: true,
        posterImage: ""
      },
      vastOptions: {
        adList: [],
        adCTAText: false,
        // "adCTATextPosition": ""
      }
    });
  }

}