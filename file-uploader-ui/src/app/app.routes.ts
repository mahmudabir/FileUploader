import { Routes } from '@angular/router';
import { DownloadComponent } from './download/download.component';
import { UploadComponent } from './upload/upload.component';
import { VideoStreamComponent } from './video-stream/video-stream.component';

export const routes: Routes = [
    { path: 'upload', component: UploadComponent },
    { path: 'download', component: DownloadComponent },
    { path: 'video', component: VideoStreamComponent },

];
