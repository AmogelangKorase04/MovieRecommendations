import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from '../environments/environment'; // import environment

@Injectable({
  providedIn: 'root'
})
export class MovieService {
  private baseUrl = environment.apiBaseUrl;

  constructor(private http: HttpClient) {}

  getTopMovies(count: number): Observable<any> {
    return this.http.get(`${this.baseUrl}/Movies/top/${count}`);
  }

  getMovieByRank(rank: number): Observable<any> {
    return this.http.get(`${this.baseUrl}/Movies/${rank}`);
  }

  getMoviesByYear(year: number): Observable<any> {
    return this.http.get(`${this.baseUrl}/Movies/year/${year}`);
  }

  getPopularMovies(): Observable<any> {
    return this.http.get(`${this.baseUrl}/Movies/popular`);
  }

  getCriticallyAcclaimed(): Observable<any> {
    return this.http.get(`${this.baseUrl}/Movies/critically-acclaimed`);
  }

  getFamilyFriendly(): Observable<any> {
    return this.http.get(`${this.baseUrl}/Movies/family-friendly`);
  }

  getQuickWatch(): Observable<any> {
    return this.http.get(`${this.baseUrl}/Movies/quick-watch`);
  }

  getRatingVsPopularity(): Observable<any> {
    return this.http.get(`${this.baseUrl}/Analytics/rating-vs-popularity`);
  }

  getByDecade(): Observable<any> {
    return this.http.get(`${this.baseUrl}/Analytics/by-decade`);
  }

  getStatistics(): Observable<any> {
    return this.http.get(`${this.baseUrl}/Analytics/statistics`);
  }
}
