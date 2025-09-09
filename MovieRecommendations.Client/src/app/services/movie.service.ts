import { Injectable } from '@angular/core';
import { HttpClient, HttpParams } from '@angular/common/http';
import { Observable } from 'rxjs';
import { Movie, MovieFilter, MovieStatistics } from '../models/movie.model';
import { environment } from '../../environments/environment';

@Injectable({
  providedIn: 'root'
})
export class MovieService {
  private readonly apiUrl = environment.apiUrl;

  constructor(private http: HttpClient) { }

  // Basic CRUD operations
  getAllMovies(): Observable<Movie[]> {
    return this.http.get<Movie[]>(`${this.apiUrl}/movies`);
  }

  getMovieByRank(rank: number): Observable<Movie> {
    return this.http.get<Movie>(`${this.apiUrl}/movies/${rank}`);
  }

  getMoviesByYear(year: number): Observable<Movie[]> {
    return this.http.get<Movie[]>(`${this.apiUrl}/movies/year/${year}`);
  }

  getTopRated(count: number = 10): Observable<Movie[]> {
    return this.http.get<Movie[]>(`${this.apiUrl}/movies/top/${count}`);
  }

  // Filtering methods
  filterByRating(minRating: number = 0, maxRating: number = 10): Observable<Movie[]> {
    const params = new HttpParams()
      .set('minRating', minRating.toString())
      .set('maxRating', maxRating.toString());

    return this.http.get<Movie[]>(`${this.apiUrl}/movies/filter/rating`, { params });
  }

  filterByYearRange(startYear: number, endYear: number): Observable<Movie[]> {
    const params = new HttpParams()
      .set('startYear', startYear.toString())
      .set('endYear', endYear.toString());

    return this.http.get<Movie[]>(`${this.apiUrl}/movies/filter/year-range`, { params });
  }

  filterByDuration(maxMinutes: number): Observable<Movie[]> {
    const params = new HttpParams().set('maxMinutes', maxMinutes.toString());
    return this.http.get<Movie[]>(`${this.apiUrl}/movies/filter/duration`, { params });
  }

  filterByAgeRating(ageLimit: string): Observable<Movie[]> {
    return this.http.get<Movie[]>(`${this.apiUrl}/movies/filter/age-rating/${ageLimit}`);
  }

  // Search
  searchMovies(query: string): Observable<Movie[]> {
    const params = new HttpParams().set('query', query);
    return this.http.get<Movie[]>(`${this.apiUrl}/movies/search`, { params });
  }

  // Special categories
  getMostPopular(count: number = 10): Observable<Movie[]> {
    const params = new HttpParams().set('count', count.toString());
    return this.http.get<Movie[]>(`${this.apiUrl}/movies/popular`, { params });
  }

  getCriticallyAcclaimed(minMetascore: number = 80): Observable<Movie[]> {
    const params = new HttpParams().set('minMetascore', minMetascore.toString());
    return this.http.get<Movie[]>(`${this.apiUrl}/movies/critically-acclaimed`, { params });
  }

  getFamilyFriendly(): Observable<Movie[]> {
    return this.http.get<Movie[]>(`${this.apiUrl}/movies/family-friendly`);
  }

  getQuickWatch(maxMinutes: number = 120): Observable<Movie[]> {
    const params = new HttpParams().set('maxMinutes', maxMinutes.toString());
    return this.http.get<Movie[]>(`${this.apiUrl}/movies/quick-watch`, { params });
  }

  // Analytics
  getStatistics(): Observable<MovieStatistics> {
    return this.http.get<MovieStatistics>(`${this.apiUrl}/analytics/statistics`);
  }

  getByDecade(): Observable<any[]> {
    return this.http.get<any[]>(`${this.apiUrl}/analytics/by-decade`);
  }

  getRatingVsPopularity(): Observable<any[]> {
    return this.http.get<any[]>(`${this.apiUrl}/analytics/rating-vs-popularity`);
  }

  // Recommendations
  getSimilarByYear(movieId: number, count: number = 5): Observable<Movie[]> {
    const params = new HttpParams().set('count', count.toString());
    return this.http.get<Movie[]>(`${this.apiUrl}/recommendations/similar-year/${movieId}`, { params });
  }

  getHiddenGems(count: number = 10): Observable<Movie[]> {
    const params = new HttpParams().set('count', count.toString());
    return this.http.get<Movie[]>(`${this.apiUrl}/recommendations/hidden-gems`, { params });
  }

  getCrowdPleasers(count: number = 10): Observable<Movie[]> {
    const params = new HttpParams().set('count', count.toString());
    return this.http.get<Movie[]>(`${this.apiUrl}/recommendations/crowd-pleasers`, { params });
  }

  getTimeBasedRecommendations(availableMinutes: number = 120): Observable<Movie[]> {
    const params = new HttpParams().set('availableMinutes', availableMinutes.toString());
    return this.http.get<Movie[]>(`${this.apiUrl}/recommendations/time-based`, { params });
  }
}
