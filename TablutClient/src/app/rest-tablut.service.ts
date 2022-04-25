import { Injectable } from '@angular/core';
import { HttpClient, HttpHeaders } from '@angular/common/http';


/**
 * server module accesses the crud  rest api providede by the web-api
 */
@Injectable({
  providedIn: 'root'
})
export class RestTablutService {
  private REST_API_SERVER = "https://localhost:44386/api";

  constructor(private httpClient: HttpClient) { }

  public sendGetRequest(api){
    return this.httpClient.get(this.REST_API_SERVER+"/"+api);
  }

  public sendPostRequest(api, body: any){
    return this.httpClient.post<any>(this.REST_API_SERVER+"/"+api, body);
  }

  public sendPutRequest(api,id, move: any){
    return this.httpClient.put<any>(this.REST_API_SERVER+"/"+api + "/" + id, move);
  }
}
