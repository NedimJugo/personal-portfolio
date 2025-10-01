import { ProjectImage } from "../project-image/project-image.model"
import { TagResponse } from "../tag/tag-response.model"
import { TechResponse } from "../tech/tech-response.model"
import { ProjectResponse } from "./project-response.model"

export interface ProjectDetails extends ProjectResponse {
  images: ProjectImage[]
  tags: TagResponse[]
  techs: TechResponse[]
}