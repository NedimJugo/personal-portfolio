import { ExperienceResponse } from "./experience-response.model"

export interface ParsedExperience extends ExperienceResponse {
  achievementsList: string[]
  technologiesList: string[]
}