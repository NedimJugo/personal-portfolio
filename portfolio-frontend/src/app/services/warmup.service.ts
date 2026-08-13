import { Injectable } from '@angular/core';
import { BehaviorSubject } from 'rxjs';

@Injectable({
  providedIn: 'root'
})
export class WarmupService {
  private isFirstRequest = true;
  private isWarmingUp = new BehaviorSubject<boolean>(false);
  
  isWarmingUp$ = this.isWarmingUp.asObservable();

  startWarmup() {
    if (this.isFirstRequest) {
      this.isWarmingUp.next(true);
      this.isFirstRequest = false;
    }
  }

  completeWarmup() {
    this.isWarmingUp.next(false);
  }

  reset() {
    this.isFirstRequest = true;
    this.isWarmingUp.next(false);
  }
}