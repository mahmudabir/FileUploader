import { CommonModule } from '@angular/common';
import { HttpClient, HttpEvent, HttpEventType } from '@angular/common/http';
import { ChangeDetectionStrategy, Component } from '@angular/core';
import { Observable, map } from 'rxjs';
import { environment } from '../../environments/environment';

@Component({
  selector: 'app-download',
  standalone: true,
  imports: [
    CommonModule,
  ],
  templateUrl: `./download.component.html`,
  styleUrl: './download.component.css',
  changeDetection: ChangeDetectionStrategy.Default,
})
export class DownloadComponent {

  baseUrl = `${environment.apiBaseUrl}/api`;

  downloadProgress: number = 0;

  constructor(private http: HttpClient) {

  }


  // Method to download file with progress tracking
  downloadFile(fileName: string): Observable<any> {
    const url = `${this.baseUrl}/filedownload/download/${fileName}`;
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
            this.downloadProgress = progress;
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

    if (!isUiDownload) {
      window.open(`${this.baseUrl}/api/filedownload/download/${fileName}`, '_blank');
    } else {
      this.downloadFile(fileName).subscribe({
        next: (blob) => {

          try {
            if (blob.status == 'progress') {

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

              alert("File downloaded successfully");
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
}
