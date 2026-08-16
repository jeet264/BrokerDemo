import { ImportWizard } from './ImportWizard'

/** Client spreadsheet import at /clients/import. */
export function ClientImportPage() {
  return <ImportWizard kind="clients" />
}

/** Policy spreadsheet import at /policies/import. */
export function PolicyImportPage() {
  return <ImportWizard kind="policies" />
}
