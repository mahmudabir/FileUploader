import { CommonModule } from '@angular/common';
import { HttpClient } from '@angular/common/http';
import { Component, ElementRef, OnInit, ViewChild } from '@angular/core';
import fluidPlayer from 'fluid-player';

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

  videoUrl = 'https://localhost:7001/api/hls/master';

  constructor(private http: HttpClient) {
    setTimeout(() => {
      this.initializeFluidPlayer();
    }, 0);
  }

  ngOnInit() {
  }

  ngAfterViewInit() {
    this.initializeFluidPlayer();
  }

  initializeFluidPlayer() {
    fluidPlayer(
      'videoElement', {
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