import { CommonModule } from '@angular/common';
import { HttpClient } from '@angular/common/http';
import { ChangeDetectionStrategy, Component, Input, OnInit } from '@angular/core';
import fluidPlayer from 'fluid-player';

@Component({
  selector: 'app-video-player',
  standalone: true,
  imports: [
    CommonModule,
  ],
  templateUrl: `./video-player.component.html`,
  styleUrl: './video-player.component.css',
  changeDetection: ChangeDetectionStrategy.Default,
})
export class VideoPlayerComponent implements OnInit {

  public player: FluidPlayerInstance;

  masterPlaylist: any;

  @Input() url: string;

  @Input() options?: Partial<FluidPlayerOptions>;

  defaultOptions: Partial<FluidPlayerOptions> = {
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
  };

  constructor(private http: HttpClient) {


  }
  ngOnInit(): void {
    // this.http.get(this.url, { responseType: 'blob' })
    //   .subscribe(res => {
    //     this.masterPlaylist = URL.createObjectURL(res);

    //     setTimeout(() => {
    //       this.initializeFluidPlayer();
    //     }, 1);
    //   });

      setTimeout(() => {
        this.initializeFluidPlayer();
      }, 1);
  }

  initializeFluidPlayer(options?: Partial<FluidPlayerOptions>) {
    this.player = fluidPlayer('videoElement', options ?? this.defaultOptions);
  }

}
