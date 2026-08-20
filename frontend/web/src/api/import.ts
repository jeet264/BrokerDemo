import { http, postApiData, postFormData } from './client'
import type { ImportCommitResult, ImportPreview, ClientImportValues, PolicyImportValues } from '../types/api'

/** POST /api/import/clients/preview — multipart file, does not save. */
export function previewClientImport(file: File) {
  const form = new FormData()
  form.append('file', file)
  return postFormData<ImportPreview<ClientImportValues>>('/api/import/clients/preview', form)
}

/** POST /api/import/clients/confirm — JSON { previewToken }, inserts valid rows only. */
export function confirmClientImport(previewToken: string) {
  return postApiData<ImportCommitResult>('/api/import/clients/confirm', { previewToken })
}

/** POST /api/import/policies/preview?matchBy=ClientCode|NameAndPhone */
export function previewPolicyImport(file: File, matchBy: 'ClientCode' | 'NameAndPhone') {
  const form = new FormData()
  form.append('file', file)
  return postFormData<ImportPreview<PolicyImportValues>>(
    `/api/import/policies/preview?matchBy=${encodeURIComponent(matchBy)}`,
    form,
  )
}

/** POST /api/import/policies/confirm */
export function confirmPolicyImport(previewToken: string, matchBy: 'ClientCode' | 'NameAndPhone') {
  return postApiData<ImportCommitResult>(
    `/api/import/policies/confirm?matchBy=${encodeURIComponent(matchBy)}`,
    { previewToken },
  )
}

/** GET /api/import/{clients|policies}/template — raw .xlsx, not the JSON envelope. */
export async function downloadImportTemplate(kind: 'clients' | 'policies'): Promise<void> {
  const response = await http.get(`/api/import/${kind}/template`, {
    responseType: 'blob',
    timeout: 30000,
  })

  const blob = new Blob([response.data], {
    type: 'application/vnd.openxmlformats-officedocument.spreadsheetml.sheet',
  })
  const url = URL.createObjectURL(blob)
  const link = document.createElement('a')
  link.href = url
  link.download = kind === 'clients' ? 'InsuOrg-clients-template.xlsx' : 'InsuOrg-policies-template.xlsx'
  document.body.appendChild(link)
  link.click()
  link.remove()
  URL.revokeObjectURL(url)
}
