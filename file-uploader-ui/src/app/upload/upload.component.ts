import { CommonModule } from '@angular/common';
import { HttpClient, HttpEventType } from '@angular/common/http';
import { ChangeDetectionStrategy, Component } from '@angular/core';
import { Subject, Observable, of } from 'rxjs';

@Component({
  selector: 'app-upload',
  standalone: true,
  imports: [
    CommonModule,
  ],
  templateUrl: './upload.component.html',
  styleUrl: './upload.component.css',
  changeDetection: ChangeDetectionStrategy.Default,
})
export class UploadComponent {
  uploadProgress: number = 0;
  private chunkSize = 1024 * 1024 * 25; // 25MB

  private selectedFile?: File;

  constructor(private http: HttpClient) {

  }

  onFileSelected(event: any) {
    const file = event.target.files[0];
    if (file) {
      this.selectedFile = file;
      this.uploadProgress = 0;
    }
  }

  upload(isFullUpload: boolean) {
    this.uploadFile(isFullUpload, this.selectedFile).subscribe({
      error: (err) => {
        console.log('Upload failed:', err);
        alert('Upload failed');
      },
      complete: () => alert('Upload complete!')
    });
  }

  // Method to upload a file in chunks
  uploadFile(isFullUpload: boolean, file?: File): Observable<any> {

    if (!file) {
      return of();
    }

    const totalChunks = Math.ceil(file.size / this.chunkSize);

    let uploadObservable: Observable<any> = new Observable(observer => {
      const uploadChunk = async (chunkNumber: number) => {
        if (chunkNumber > totalChunks) {
          observer.complete();
          return;
        }

        const start = (chunkNumber - 1) * this.chunkSize;
        const end = Math.min(file.size, start + this.chunkSize);
        const chunk = file.slice(start, end);

        const formData = new FormData();
        formData.append('chunk', chunk);
        formData.append('fileName', file.name);
        formData.append('chunkNumber', chunkNumber.toString());
        formData.append('totalChunks', totalChunks.toString());

        this.http.post(`https://localhost:7001/api/FileUpload/upload-${(isFullUpload ? 'full' : 'chunk')}`, formData, { reportProgress: true, observe: 'events' })
          .subscribe({
            next: event => {
              if (event.type === HttpEventType.UploadProgress) {
                const percentDone = Math.round(100 * chunkNumber / totalChunks);
                // this.progressSubject.next(percentDone);
                this.uploadProgress = percentDone;
              }
              if (event.type === HttpEventType.Response) {
                uploadChunk(chunkNumber + 1);
              }
            },
            error: error => observer.error(error)
          });
      };

      uploadChunk(1);
    });

    this.uploadProgress = 0;
    return uploadObservable;
  }
}
