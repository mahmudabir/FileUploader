import { CommonModule } from '@angular/common';
import { ChangeDetectionStrategy, Component } from '@angular/core';

@Component({
  selector: 'app-video',
  standalone: true,
  imports: [
    CommonModule,
  ],
  templateUrl: `./video.component.html`,
  styleUrl: './video.component.css',
  changeDetection: ChangeDetectionStrategy.Default,
})
export class VideoComponent { }
