import { Component, Input } from '@angular/core';
import { CommonModule } from '@angular/common';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';

@Component({
  selector: 'app-loading',
  standalone: true,
  imports: [CommonModule, MatProgressSpinnerModule],
  template: `
    <div class="loading-container" [class.fullscreen]="fullscreen">
      <mat-spinner [diameter]="size"></mat-spinner>
      <p *ngIf="message" class="loading-message">{{ message }}</p>
    </div>
  `,
  styles: [`
    .loading-container {
      display: flex;
      flex-direction: column;
      align-items: center;
      justify-content: center;
      padding: 2rem;
    }
    
    .loading-container.fullscreen {
      position: fixed;
      top: 0;
      left: 0;
      width: 100vw;
      height: 100vh;
      background: rgba(255, 255, 255, 0.9);
      z-index: 9999;
    }
    
    .loading-message {
      margin-top: 1rem;
      color: #666;
      font-size: 0.9rem;
    }
  `]
})
export class Loading {
  @Input() message: string = '';
  @Input() size: number = 40;
  @Input() fullscreen: boolean = false;
}