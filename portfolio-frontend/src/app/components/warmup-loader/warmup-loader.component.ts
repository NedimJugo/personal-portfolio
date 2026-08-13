import { Component, OnInit, OnDestroy } from '@angular/core';
import { CommonModule } from '@angular/common';
import { WarmupService } from '../../services/warmup.service';
import { Subscription } from 'rxjs';

@Component({
  selector: 'app-warmup-loader',
  standalone: true,
  imports: [CommonModule],
  template: `
    <div *ngIf="isLoading" class="warmup-overlay">
      <div class="warmup-modal">
        <div class="icon-container">
          <!-- Server Icon -->
          <svg class="server-icon" xmlns="http://www.w3.org/2000/svg" width="64" height="64" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round">
            <rect x="2" y="2" width="20" height="8" rx="2" ry="2"></rect>
            <rect x="2" y="14" width="20" height="8" rx="2" ry="2"></rect>
            <line x1="6" y1="6" x2="6.01" y2="6"></line>
            <line x1="6" y1="18" x2="6.01" y2="18"></line>
          </svg>
          <!-- Spinner -->
          <svg class="spinner" xmlns="http://www.w3.org/2000/svg" width="24" height="24" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round">
            <line x1="12" y1="2" x2="12" y2="6"></line>
            <line x1="12" y1="18" x2="12" y2="22"></line>
            <line x1="4.93" y1="4.93" x2="7.76" y2="7.76"></line>
            <line x1="16.24" y1="16.24" x2="19.07" y2="19.07"></line>
            <line x1="2" y1="12" x2="6" y2="12"></line>
            <line x1="18" y1="12" x2="22" y2="12"></line>
            <line x1="4.93" y1="19.07" x2="7.76" y2="16.24"></line>
            <line x1="16.24" y1="7.76" x2="19.07" y2="4.93"></line>
          </svg>
        </div>

        <div class="content">
          <h3>WAKING UP THE SERVER...</h3>
          <p>Our backend is hosted on Render's free tier and may take up to 60-80 seconds to start up if it's been inactive.</p>
        </div>

        <div class="timer">
          <svg xmlns="http://www.w3.org/2000/svg" width="16" height="16" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round">
            <circle cx="12" cy="12" r="10"></circle>
            <polyline points="12 6 12 12 16 14"></polyline>
          </svg>
          <span>Elapsed time: {{ elapsed }}s</span>
        </div>

        <div class="progress-bar">
          <div class="progress-fill" [style.width.%]="progress"></div>
        </div>

        <div *ngIf="showSlowWarning" class="warning">
          <p class="warning-title">Taking longer than expected?</p>
          <p class="warning-text">The server might be experiencing delays. Please wait a bit longer or try refreshing.</p>
        </div>

        <p class="footer">Thank you for your patience!</p>
      </div>
    </div>
  `,
  styles: [`
    @use "sass:color";

    // Comic Color Palette (matching your app)
    $comic-yellow: #FFD93D;
    $comic-red: #FF6B6B;
    $comic-blue: #4ECDC4;
    $comic-purple: #A78BFA;
    $comic-dark: #2D3748;
    $comic-black: #1A202C;
    $comic-white: #FFFFFF;
    $comic-cream: #FFF8E7;
    $comic-gray: #718096;

    .warmup-overlay {
      position: fixed;
      inset: 0;
      background-color: rgba(26, 32, 44, 0.7);
      backdrop-filter: blur(4px);
      display: flex;
      align-items: center;
      justify-content: center;
      z-index: 9999;
      padding: 1rem;
      animation: fadeIn 0.3s ease-out;
    }

    @keyframes fadeIn {
      from { opacity: 0; }
      to { opacity: 1; }
    }

    .warmup-modal {
      background: $comic-white;
      border-radius: 12px;
      border: 4px solid $comic-black;
      box-shadow: 8px 8px 0 $comic-black;
      max-width: 28rem;
      width: 100%;
      padding: 2rem;
      display: flex;
      flex-direction: column;
      gap: 1.5rem;
      position: relative;
      animation: modalSlide 0.3s cubic-bezier(0.175, 0.885, 0.32, 1.275);

      &::before {
        content: '';
        position: absolute;
        top: 0;
        left: 0;
        right: 0;
        bottom: 0;
        background-image: radial-gradient(circle, $comic-black 1px, transparent 1px);
        background-size: 8px 8px;
        opacity: 0.03;
        pointer-events: none;
        border-radius: 8px;
      }

      @media (max-width: 768px) {
        padding: 1.5rem;
        gap: 1.25rem;
      }
    }

    @keyframes modalSlide {
      from {
        opacity: 0;
        transform: translateY(-20px) scale(0.95);
      }
      to {
        opacity: 1;
        transform: translateY(0) scale(1);
      }
    }

    .icon-container {
      display: flex;
      align-items: center;
      justify-content: center;
      position: relative;
      margin-bottom: 0.5rem;
    }

    .server-icon {
      color: $comic-blue;
      filter: drop-shadow(3px 3px 0 rgba(26, 32, 44, 0.2));
    }

    .spinner {
      color: $comic-blue;
      position: absolute;
      top: -0.25rem;
      right: calc(50% - 3.5rem);
      animation: spin 1s linear infinite;
      filter: drop-shadow(2px 2px 0 rgba(26, 32, 44, 0.15));
    }

    @keyframes spin {
      from { transform: rotate(0deg); }
      to { transform: rotate(360deg); }
    }

    .content {
      text-align: center;
      position: relative;
      z-index: 1;
    }

    .content h3 {
      font-size: 1.5rem;
      font-weight: 900;
      color: $comic-black;
      margin: 0 0 1rem 0;
      text-transform: uppercase;
      letter-spacing: -0.02em;
      line-height: 1.2;

      @media (max-width: 768px) {
        font-size: 1.25rem;
      }
    }

    .content p {
      color: $comic-gray;
      margin: 0;
      line-height: 1.6;
      font-size: 15px;
      font-weight: 500;
    }

    .timer {
      display: flex;
      align-items: center;
      justify-content: center;
      gap: 0.5rem;
      font-size: 0.875rem;
      color: $comic-gray;
      font-weight: 700;
      text-transform: uppercase;
      letter-spacing: 0.5px;
      position: relative;
      z-index: 1;

      svg {
        color: $comic-blue;
      }
    }

    .progress-bar {
      width: 100%;
      background-color: $comic-cream;
      border: 3px solid $comic-black;
      border-radius: 9999px;
      height: 1rem;
      overflow: hidden;
      position: relative;
      z-index: 1;
      box-shadow: inset 2px 2px 4px rgba(0, 0, 0, 0.1);
    }

    .progress-fill {
      background: linear-gradient(135deg, $comic-blue 0%, $comic-purple 100%);
      height: 100%;
      transition: width 1s cubic-bezier(0.175, 0.885, 0.32, 1.275);
      border-radius: 9999px;
      box-shadow: 0 2px 4px rgba(78, 205, 196, 0.4);
    }

    .warning {
      background: linear-gradient(135deg, 
        #{color.adjust($comic-yellow, $lightness: 35%)} 0%, 
        #{color.adjust($comic-red, $lightness: 40%)} 100%
      );
      border: 3px solid $comic-black;
      border-radius: 8px;
      padding: 1rem;
      position: relative;
      z-index: 1;
      box-shadow: 4px 4px 0 rgba(26, 32, 44, 0.15);
    }

    .warning-title {
      font-weight: 800;
      color: $comic-black;
      margin: 0 0 0.5rem 0;
      font-size: 0.875rem;
      text-transform: uppercase;
      letter-spacing: 0.5px;
    }

    .warning-text {
      color: $comic-dark;
      margin: 0;
      font-size: 0.875rem;
      line-height: 1.5;
      font-weight: 600;
    }

    .footer {
      text-align: center;
      font-size: 0.75rem;
      color: $comic-gray;
      margin: 0;
      font-weight: 700;
      text-transform: uppercase;
      letter-spacing: 1px;
      position: relative;
      z-index: 1;
    }
  `]
})
export class WarmupLoaderComponent implements OnInit, OnDestroy {
  isLoading = false;
  elapsed = 0;
  progress = 0;
  showSlowWarning = false;
  
  private subscription?: Subscription;
  private timerInterval?: any;
  private warningTimeout?: any;

  constructor(private warmupService: WarmupService) {}

  ngOnInit() {
    this.subscription = this.warmupService.isWarmingUp$.subscribe(isWarming => {
      this.isLoading = isWarming;
      
      if (isWarming) {
        this.startTimer();
      } else {
        this.stopTimer();
      }
    });
  }

  ngOnDestroy() {
    this.subscription?.unsubscribe();
    this.stopTimer();
  }

  private startTimer() {
    this.elapsed = 0;
    this.progress = 0;
    this.showSlowWarning = false;

    this.timerInterval = setInterval(() => {
      this.elapsed++;
      this.progress = Math.min((this.elapsed / 80) * 100, 100);
    }, 1000);

    this.warningTimeout = setTimeout(() => {
      this.showSlowWarning = true;
    }, 10000);
  }

  private stopTimer() {
    if (this.timerInterval) {
      clearInterval(this.timerInterval);
      this.timerInterval = undefined;
    }
    if (this.warningTimeout) {
      clearTimeout(this.warningTimeout);
      this.warningTimeout = undefined;
    }
    this.elapsed = 0;
    this.progress = 0;
    this.showSlowWarning = false;
  }
}