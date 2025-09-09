export interface Movie {
  rank: number;
  year: number;
  duration: string;
  ageLimit: string;
  rating: number;
  numberOfRatings: number;
  metascore?: number;
  description: string;
  name: string;
}

export interface MovieFilter {
  minRating?: number;
  maxRating?: number;
  startYear?: number;
  endYear?: number;
  ageLimit?: string;
  maxDuration?: number;
  searchQuery?: string;
}

export interface MovieStatistics {
  totalMovies: number;
  averageRating: number;
  yearRange: {
    min: number;
    max: number;
  };
  mostPopular: string;
  highestRated: string;
  ratingDistribution: { [key: string]: number };
}
