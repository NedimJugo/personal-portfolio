import { Injectable } from '@angular/core';
import { Observable } from 'rxjs';
import { ApiService } from './api';
import { Project } from '../models/project';
import { ApiResponse, PaginatedResponse } from '../models/api-response';

@Injectable({
  providedIn: 'root'
})
export class ProjectService {
  constructor(private apiService: ApiService) {}

  // Public endpoints
  getProjects(params?: {
    page?: number;
    pageSize?: number;
    tag?: string;
    tech?: string;
    search?: string;
  }): Observable<PaginatedResponse<Project>> {
    return this.apiService.get<PaginatedResponse<Project>>('/projects', params);
  }

  getProject(slug: string): Observable<ApiResponse<Project>> {
    return this.apiService.get<ApiResponse<Project>>(`/projects/${slug}`);
  }

  // Admin endpoints
  getAdminProjects(): Observable<ApiResponse<Project[]>> {
    return this.apiService.get<ApiResponse<Project[]>>('/admin/projects');
  }

  createProject(project: Partial<Project>): Observable<ApiResponse<Project>> {
    return this.apiService.post<ApiResponse<Project>>('/admin/projects', project);
  }

  updateProject(id: number, project: Partial<Project>): Observable<ApiResponse<Project>> {
    return this.apiService.put<ApiResponse<Project>>(`/admin/projects/${id}`, project);
  }

  deleteProject(id: number): Observable<ApiResponse<void>> {
    return this.apiService.delete<ApiResponse<void>>(`/admin/projects/${id}`);
  }
}