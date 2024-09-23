import { CommonModule } from '@angular/common';
import { HttpClient, HttpEvent, HttpEventType } from '@angular/common/http';
import { Component } from '@angular/core';
import { RouterOutlet } from '@angular/router';
import { map, Observable, Subject } from 'rxjs';

@Component({
  selector: 'app-root',
  standalone: true,
  imports: [RouterOutlet, CommonModule],
  templateUrl: './app.component.html',
  styleUrl: './app.component.css'
})
export class AppComponent {
  title = 'file-uploader-ui';
  uploadProgress: number = 0;
  downloadProgress: number = 0;
  private chunkSize = 1024 * 1024 * 25; // 25MB

  private progressSubject = new Subject<number>();

  constructor(private http: HttpClient) {

  }

  // Method to get progress updates
  getProgress(): Observable<number> {
    return this.progressSubject.asObservable();
  }

  onFileSelected(event: any) {
    const file = event.target.files[0];
    if (file) {
      this.uploadProgress = 0;
      this.getProgress().subscribe(percent => {
        this.uploadProgress = percent;
      });

      this.uploadFile(file).subscribe({
        error: (err) => console.log('Upload failed:', err),
        complete: () => console.log('Upload complete!')
      });
    }
  }

  // Method to upload a file in chunks
  uploadFile(file: File): Observable<any> {
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

        this.http.post('https://localhost:7001/api/FileUpload/upload-chunk', formData, { reportProgress: true, observe: 'events' })
          .subscribe({
            next: event => {
              if (event.type === HttpEventType.UploadProgress) {
                const percentDone = Math.round(100 * chunkNumber / totalChunks);
                this.progressSubject.next(percentDone);
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

  // Method to download file with progress tracking
  downloadFile(fileName: string): Observable<any> {
    const url = `https://localhost:7001/api/filedownload/download/${fileName}`;
    return this.http.get(url, {
      responseType: 'blob',  // To get the file as a blob
      observe: 'events',     // To get the download progress events
      reportProgress: true   // Track the progress
    }).pipe(
      map((event: HttpEvent<any>) => {
        switch (event.type) {
          case HttpEventType.DownloadProgress:
            // Calculate the percentage
            const progress = event.total ? Math.round((event.loaded / event.total) * 100) : 0;
            return { status: 'progress', message: progress };

          case HttpEventType.Response:
            this.downloadProgress = 0;
            // When the response is complete
            return { status: 'completed', blob: event.body };

          default:
            return { status: 'event', message: event };
        }
      })
    );
  }

  download(fileName: string = "", isUiDownload: boolean = false) {

    // 'Wednesday S01E02  1080p NF WEBRip x265 HEVC MSubs [Dual Audio][Hindi 5.1+English 5.1] -OlaM.mkv'

    if (!isUiDownload) {
      window.open(`https://localhost:7001/api/filedownload/download/${fileName}`, '_blank');
    } else {
      this.downloadFile(fileName).subscribe({
        next: (blob) => {
  
          try {
            if (blob.status == 'progress') {
              this.downloadProgress = blob.message;
            } else if (blob.status == 'completed') {
              // Create a URL for the blob data
              const blobUrl = window.URL.createObjectURL(blob.blob);
  
              // Create a temporary anchor element
              const link = document.createElement('a');
              link.href = blobUrl;
              link.download = fileName; // Specify the file name
              // link.target = "_blank";
  
              // Append the anchor to the body (required for Firefox)
              document.body.appendChild(link);
  
              // Programmatically click the anchor to trigger the download
              link.click();
  
              // Remove the anchor from the DOM
              document.body.removeChild(link);
  
              // Revoke the blob URL to free memory
              URL.revokeObjectURL(blobUrl);
            } else {
  
            }
          } catch (error: any) {
            // console.log(error.message);
          }
        },
        error: (error) => {
          // console.log('File download failed:', error);
          alert('File download failed');
        }
      });
    }

  }

  generateFileSourceUrlFromBlobData(data: BlobPart | BlobPart[]): string {
    let blobParts: BlobPart[] = Array.isArray(data) ? data : [data];
    const blob = new Blob(blobParts, { type: '*/*' }); // Change the type based on your content type
    return window.URL.createObjectURL(blob) // generate image src for client side
  }

}
