import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { MovieService } from '../../services/movie.service';
import { NgFor, NgIf, AsyncPipe } from '@angular/common';

@Component({
  selector: 'app-dashboard',
  standalone: true,
  imports: [CommonModule, NgFor, NgIf, AsyncPipe],
  templateUrl: './dashboard.component.html',
  styleUrls: ['./dashboard.component.css']
})
export class DashboardComponent implements OnInit {

  topMovies$ = this.movieService.getTopMovies(5);
  popularMovies$ = this.movieService.getPopularMovies();
  criticallyAcclaimed$ = this.movieService.getCriticallyAcclaimed();
  familyFriendly$ = this.movieService.getFamilyFriendly();
  quickWatch$ = this.movieService.getQuickWatch();

  ratingVsPopularity$ = this.movieService.getRatingVsPopularity();
  byDecade$ = this.movieService.getByDecade();
  statistics$ = this.movieService.getStatistics();

  constructor(private movieService: MovieService) {}

  ngOnInit(): void {
    // All data fetching is handled via async pipe
  }
}
