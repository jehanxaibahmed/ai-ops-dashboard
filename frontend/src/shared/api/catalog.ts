import { request } from './http'
import type { Catalog } from './types'

export function getCatalog() {
  return request<Catalog>('/api/catalog')
}
