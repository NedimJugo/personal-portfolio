export interface BaseSearchObject {
  page?: number;
  pageSize?: number;
  sortBy?: string;
  desc?: boolean;
  includeTotalCount?: boolean;
  retrieveAll?: boolean;
}
