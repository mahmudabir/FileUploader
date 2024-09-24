import { Routes } from '@angular/router';
import { DownloadComponent } from './download/download.component';
import { UploadComponent } from './upload/upload.component';
import { VideoPlayerComponent } from './video-player/video-player.component';

export const routes: Routes = [
    { path: 'upload', component: UploadComponent },
    { path: 'download', component: DownloadComponent },
    { path: 'video', component: VideoPlayerComponent },

];
