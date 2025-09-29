import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { BaseCrudService } from './base/base-crud.service';
import { TestimonialResponse } from '../models/testimonial/testimonial-response.model';
import { TestimonialSearchObject } from '../models/testimonial/testimonial-search.model';
import { TestimonialInsertRequest } from '../models/testimonial/testimonial-insert-request.model';
import { TestimonialUpdateRequest } from '../models/testimonial/testimonial-update-request.model';

@Injectable({
  providedIn: 'root'
})
export class TestimonialService extends BaseCrudService<
  TestimonialResponse,
  TestimonialSearchObject,
  TestimonialInsertRequest,
  TestimonialUpdateRequest,
  string
> {
  
  protected endpoint = 'testimonials';

  constructor(http: HttpClient) {
    super(http);
  }
}