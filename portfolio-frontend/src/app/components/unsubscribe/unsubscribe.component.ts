import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ActivatedRoute } from '@angular/router';
import { HttpClient } from '@angular/common/http';
import { environment } from '../../../environments/environment';

@Component({
  selector: 'app-unsubscribe',
  standalone: true,
  imports: [CommonModule],
  templateUrl: './unsubscribe.component.html',
  styleUrls: ['./unsubscribe.component.scss']
})
export class UnsubscribeComponent implements OnInit {
  isLoading = true;
  success = false;
  errorMessage = '';

  constructor(
    private route: ActivatedRoute,
    private http: HttpClient
  ) {}

  ngOnInit() {
    const token = this.route.snapshot.queryParams['token'];
    
    if (!token) {
      this.isLoading = false;
      this.errorMessage = 'Invalid unsubscribe link. No token provided.';
      return;
    }

    this.http.get(`${environment.apiUrl}/unsubscribe?token=${token}`)
      .subscribe({
        next: () => {
          this.isLoading = false;
          this.success = true;
        },
        error: (err) => {
          this.isLoading = false;
          this.errorMessage = err.error?.message || 'Failed to unsubscribe. Please try again.';
        }
      });
  }
}