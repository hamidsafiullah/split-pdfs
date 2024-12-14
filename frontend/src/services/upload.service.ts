import { HttpClient, HttpParams } from '@angular/common/http';
import { Injectable } from '@angular/core';
import { Observable } from 'rxjs/internal/Observable';
import { apiPath } from '../envrionment';

@Injectable({
  providedIn: 'root'
})
export class UploadService {
  private _apiPath = apiPath + 'api/Pdf';

  constructor(private http: HttpClient) {
  }

  uploadFile(file: File): Observable<any> {
    const formData = new FormData();
    formData.append('file', file);
    return this.http.post(this._apiPath + '/Upload', formData, { reportProgress: true, observe: 'events' });
  }

  viewInFornax(url: string): Observable<any> {
    console.log("url", url);

    return this.http.post(`${this._apiPath}/open-in-fornax`, { url });
  }
}
