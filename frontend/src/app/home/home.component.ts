import { Component, OnInit } from '@angular/core';
import { MatButtonModule } from '@angular/material/button';
import { MatCardModule } from '@angular/material/card';
import { MatIconModule } from '@angular/material/icon';
import { HttpEventType } from '@angular/common/http';
import { MatProgressBarModule } from '@angular/material/progress-bar';

import { MatSnackBar, MatSnackBarModule } from '@angular/material/snack-bar';
import { CommonModule } from '@angular/common';

import { UploadService } from '../../services/upload.service';
import { NgIf } from '@angular/common';
import * as signalR from '@microsoft/signalr';
import { NotificationService } from '../../services/notification.service';

@Component({
  selector: 'app-home',
  imports: [
    NgIf,
    MatButtonModule,
    MatCardModule,
    MatIconModule,
    MatProgressBarModule,
    MatSnackBarModule,
    CommonModule
  ],
  templateUrl: './home.component.html',
  styleUrl: './home.component.scss'
})

export class HomeComponent implements OnInit {
  public progress: number = 0;
  public isDraggingOver: boolean = false;
  public messages: string[] = [];
  public splitFileUrls: string[] = [];

  constructor(
    private uploadService: UploadService,
    private snackBar: MatSnackBar,
    private notificationService: NotificationService
  ) { }

  ngOnInit(): void {
    this.notificationService.setMessageCallback((message: string) => {
      this.snackBar.open(message, 'Close', { duration: 1500 });
    });
  }

  onFileSelected(event: any) {
    const file: File = event.target.files[0];
    this.uploadFile(file);

  }

  onDragOver(event: DragEvent) {
    event.preventDefault();
    this.isDraggingOver = true;
  }

  onDragLeave(event: DragEvent) {
    this.isDraggingOver = false;
  }

  onDrop(event: DragEvent) {
    event.preventDefault();
    this.isDraggingOver = false;
    const file = event.dataTransfer?.files[0];
    if (file) {
      this.uploadFile(file);
    }
  }

  uploadFile(file: File) {

    this.uploadService.uploadFile(file).subscribe({
      next: (event: any) => {
        if (event.type === HttpEventType.UploadProgress) {
          if (event.total) {
            this.progress = Math.round(100 * event.loaded / event.total);
          }
        } else if (event.type === HttpEventType.Response) {
          this.progress = 0;
          this.splitFileUrls = event.body;
          this.snackBar.open('Upload success!', 'Close', { duration: 3000 });
          console.log('Upload success', event.body);
        }
      }, error: (error: any) => {
        this.progress = 0;
        this.snackBar.open('Upload failed. Please try again.', 'Close', { duration: 3000 });
        console.error('Upload failed', error);
      },
      complete: () => {
        console.log('Upload complete');
      }
    });
  }

  viewInFornax(url: string): void {
    this.uploadService.viewInFornax(url).subscribe(response => {
      this.snackBar.open('File opened in Fornax', 'Close', { duration: 3000 });
      console.log('File opened in Fornax', response);
      const fileContent = response.fileContent;
      if (fileContent == null) {
        return;
      }
      this.openPdfInNewWindow(fileContent);
    });
  }

  openPdfInNewWindow(base64Data: string) {
    const byteCharacters = atob(base64Data);
    const byteNumbers = new Array(byteCharacters.length);
    for (let i = 0; i < byteCharacters.length; i++) {
      byteNumbers[i] = byteCharacters.charCodeAt(i);
    }
    const byteArray = new Uint8Array(byteNumbers);
    const blob = new Blob([byteArray], { type: 'application/pdf' });
    const blobUrl = URL.createObjectURL(blob);
    window.open(blobUrl, '_blank');
  }

  // viewInFornax(url: string) {
  //   this.uploadService.viewInFornax(url).subscribe((response: any) => { 
  //     this.snackBar.open('File opened in Fornax', 'Close', { duration: 3000 }); 
  //     console.log('File opened in Fornax', response); 
  //   });
  // }
}
