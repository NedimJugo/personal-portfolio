export interface SkillInsertRequest {
  name: string;
  category: string;
  proficiencyLevel: number;
  yearsExperience: number;
  isFeatured: boolean;
  iconMediaId?: string;
  color?: string;
  displayOrder: number;
}