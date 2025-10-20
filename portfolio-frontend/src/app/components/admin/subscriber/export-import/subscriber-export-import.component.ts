import { Component, OnDestroy } from '@angular/core';
import { CommonModule } from '@angular/common';
import { Subject, takeUntil, finalize } from 'rxjs';
import { SubscriberService } from '../../../../services/subscriber.service';

interface ImportProgress {
  status: 'idle' | 'processing' | 'success' | 'error';
  processed: number;
  total: number;
  successCount: number;
  errorCount: number;
  errors: string[];
}

@Component({
  selector: 'app-subscribers-export-import',
  standalone: true,
  imports: [CommonModule],
  templateUrl: './subscriber-export-import.component.html',
  styleUrls: ['./subscriber-export-import.component.scss']
})
export class SubscribersExportImportComponent implements OnDestroy {
  private destroy$ = new Subject<void>();

  isExporting = false;
  exportError: string | null = null;
  exportSuccess = false;

  importProgress: ImportProgress = {
    status: 'idle',
    processed: 0,
    total: 0,
    successCount: 0,
    errorCount: 0,
    errors: []
  };

  constructor(private subscriberService: SubscriberService) {}

  ngOnDestroy(): void {
    this.destroy$.next();
    this.destroy$.complete();
  }

  exportSubscribers(): void {
    this.isExporting = true;
    this.exportError = null;
    this.exportSuccess = false;

    this.subscriberService.get({
      page: 0,
      pageSize: 10000,
      retrieveAll: true,
      includeTotalCount: true
    })
      .pipe(
        takeUntil(this.destroy$),
        finalize(() => this.isExporting = false)
      )
      .subscribe({
        next: (result) => {
          this.generateCSV(result.items);
          this.exportSuccess = true;
          setTimeout(() => {
            this.exportSuccess = false;
          }, 5000);
        },
        error: (error) => {
          console.error('Export failed:', error);
          this.exportError = 'Failed to export subscribers. Please try again.';
        }
      });
  }

  private generateCSV(subscribers: any[]): void {
    // Create CSV headers
    const headers = ['Email', 'Name', 'IsActive', 'Source', 'SubscribedAt', 'UnsubscribedAt'];
    
    // Create CSV rows
    const rows = subscribers.map(sub => [
      sub.email,
      sub.name || '',
      sub.isActive ? 'Yes' : 'No',
      sub.source || 'Direct',
      new Date(sub.subscribedAt).toISOString(),
      sub.unsubscribedAt ? new Date(sub.unsubscribedAt).toISOString() : ''
    ]);

    // Combine headers and rows
    const csvContent = [
      headers.join(','),
      ...rows.map(row => row.map(cell => `"${cell}"`).join(','))
    ].join('\n');

    // Create and download file
    const blob = new Blob([csvContent], { type: 'text/csv;charset=utf-8;' });
    const link = document.createElement('a');
    const url = URL.createObjectURL(blob);
    const timestamp = new Date().toISOString().split('T')[0];
    
    link.setAttribute('href', url);
    link.setAttribute('download', `subscribers_export_${timestamp}.csv`);
    link.style.visibility = 'hidden';
    document.body.appendChild(link);
    link.click();
    document.body.removeChild(link);
  }

  triggerFileInput(): void {
    const fileInput = document.getElementById('fileInput') as HTMLInputElement;
    if (fileInput) {
      fileInput.click();
    }
  }

  onFileSelected(event: Event): void {
    const input = event.target as HTMLInputElement;
    if (!input.files || input.files.length === 0) {
      return;
    }

    const file = input.files[0];
    
    // Validate file type
    if (!file.name.endsWith('.csv')) {
      this.importProgress = {
        status: 'error',
        processed: 0,
        total: 0,
        successCount: 0,
        errorCount: 1,
        errors: ['Please select a valid CSV file']
      };
      return;
    }

    this.processImportFile(file);
    
    // Reset input
    input.value = '';
  }

  private processImportFile(file: File): void {
    this.importProgress = {
      status: 'processing',
      processed: 0,
      total: 0,
      successCount: 0,
      errorCount: 0,
      errors: []
    };

    const reader = new FileReader();
    
    reader.onload = (e: ProgressEvent<FileReader>) => {
      const csvText = e.target?.result as string;
      if (!csvText) {
        this.importProgress.status = 'error';
        this.importProgress.errors.push('Failed to read file');
        return;
      }

      this.parseAndImportCSV(csvText);
    };

    reader.onerror = () => {
      this.importProgress.status = 'error';
      this.importProgress.errors.push('Failed to read file');
    };

    reader.readAsText(file);
  }

  private parseAndImportCSV(csvText: string): void {
    const lines = csvText.trim().split('\n');
    
    if (lines.length < 2) {
      this.importProgress.status = 'error';
      this.importProgress.errors.push('CSV file is empty or invalid');
      return;
    }

    // Parse headers (skip first line)
    const dataLines = lines.slice(1);
    this.importProgress.total = dataLines.length;

    // Process each line
    const importRequests: any[] = [];
    
    dataLines.forEach((line, index) => {
      try {
        // Simple CSV parser (handles quoted values)
        const values = line.match(/(".*?"|[^",\s]+)(?=\s*,|\s*$)/g) || [];
        const cleanValues = values.map(v => v.replace(/^"|"$/g, '').trim());

        if (cleanValues.length < 1 || !cleanValues[0]) {
          this.importProgress.errors.push(`Line ${index + 2}: Missing email address`);
          this.importProgress.errorCount++;
          return;
        }

        const email = cleanValues[0];
        
        // Basic email validation
        if (!email.includes('@')) {
          this.importProgress.errors.push(`Line ${index + 2}: Invalid email format`);
          this.importProgress.errorCount++;
          return;
        }

        importRequests.push({
          email: email,
          name: cleanValues[1] || undefined,
          isActive: cleanValues[2] === 'Yes' || cleanValues[2] === 'true',
          source: cleanValues[3] || 'Import'
        });
        
        this.importProgress.successCount++;
      } catch (error) {
        this.importProgress.errors.push(`Line ${index + 2}: Parse error`);
        this.importProgress.errorCount++;
      }
      
      this.importProgress.processed = index + 1;
    });

    // If we have valid requests, send to API
    if (importRequests.length > 0) {
      this.bulkImportSubscribers(importRequests);
    } else {
      this.importProgress.status = 'error';
      this.importProgress.errors.push('No valid subscribers found in CSV');
    }
  }

  private bulkImportSubscribers(requests: any[]): void {
    // Since we don't have a bulk import endpoint in the service,
    // we'll simulate it or create subscribers one by one
    // For production, you should add a bulk import endpoint to your API
    
    let completed = 0;
    const total = requests.length;

    requests.forEach((request, index) => {
      this.subscriberService.create(request)
        .pipe(takeUntil(this.destroy$))
        .subscribe({
          next: () => {
            completed++;
            if (completed === total) {
              this.importProgress.status = 'success';
            }
          },
          error: (error) => {
            this.importProgress.errorCount++;
            this.importProgress.successCount--;
            this.importProgress.errors.push(`${request.email}: ${error.message}`);
            completed++;
            
            if (completed === total) {
              this.importProgress.status = this.importProgress.errorCount === total ? 'error' : 'success';
            }
          }
        });
    });
  }

  resetImport(): void {
    this.importProgress = {
      status: 'idle',
      processed: 0,
      total: 0,
      successCount: 0,
      errorCount: 0,
      errors: []
    };
  }

  getProgressPercentage(): number {
    if (this.importProgress.total === 0) return 0;
    return (this.importProgress.processed / this.importProgress.total) * 100;
  }

  goBackToOverview(): void {
        window.history.back();

    }
}