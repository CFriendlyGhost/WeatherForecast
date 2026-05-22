variable "resource_group_name" {
  description = "Name of the Azure Resource Group"
  type        = string
  default     = "rg-weatherforecast"
}

variable "location" {
  description = "Azure region for the resources"
  type        = string
  default     = "Poland Central"
}

variable "container_app_name" {
  description = "Name of the Azure Container App"
  type        = string
  default     = "ca-weatherforecast"
}

variable "docker_image" {
  description = "Docker image to deploy"
  type        = string
  default     = "mcr.microsoft.com/azuredocs/containerapps-helloworld:latest" 
}
