import { Link } from 'react-router-dom'
import { downloadImportTemplate } from '../../api/import'
import { useToast } from '../../components/feedback/ToastProvider'
import { useImportFlow, type ImportKind } from './useImportFlow'
import type { ClientImportValues, PolicyImportValues } from '../../types/api'

/**
 * Three-step bulk import wizard.
 * Props: kind = "clients" | "policies".
 * Gotcha: cover dates in the policy preview are yyyy-MM-dd strings (API DateOnly), not Date objects.
 * Invalid rows stay visible so the broker can fix the spreadsheet and re-upload; they are never imported.
 */
export function ImportWizard({ kind }: { kind: ImportKind }) {
  const { showToast } = useToast()
  const flow = useImportFlow(kind)
  const noun = kind === 'clients' ? 'clients' : 'policies'
  const listPath = kind === 'clients' ? '/clients' : '/policies'

  async function onDownloadTemplate() {
    try {
      await downloadImportTemplate(kind)
    } catch {
      showToast('Template', 'Could not download the template. Sign in as an admin or manager and try again.', 'danger')
    }
  }

  async function onConfirm() {
    await flow.runConfirm()
  }

  const preview = flow.preview
  const result = flow.result

  return (
    <div>
      <div className="page-heading d-flex justify-content-between align-items-start gap-3 flex-wrap">
        <div>
          <h2>Import {noun}</h2>
          <p>
            Preview the spreadsheet first so mistakes stay out of the live book. Only valid rows are saved, always into
            the signed-in brokerage.
          </p>
        </div>
        <Link to={listPath} className="btn btn-outline-secondary">
          Back
        </Link>
      </div>

      {flow.error && (
        <div className="alert alert-danger" role="alert">
          {flow.error}
        </div>
      )}

      {result ? (
        <ImportSummary
          kind={kind}
          imported={result.importedCount}
          skipped={result.skippedCount}
          reasons={result.skipped}
          onAgain={flow.reset}
        />
      ) : preview ? (
        <PreviewStep kind={kind} flow={flow} onConfirm={onConfirm} noun={noun} />
      ) : (
        <UploadStep kind={kind} flow={flow} onDownloadTemplate={onDownloadTemplate} noun={noun} />
      )}
    </div>
  )
}

function UploadStep({
  kind,
  flow,
  onDownloadTemplate,
  noun,
}: {
  kind: ImportKind
  flow: ReturnType<typeof useImportFlow>
  onDownloadTemplate: () => void
  noun: string
}) {
  return (
    <section className="content-card">
      <h3 className="h5">1. Upload a file</h3>
      <p className="text-muted">CSV or Excel (.xlsx). Maximum 10 MB and 2,000 rows.</p>
      {kind === 'policies' && (
        <div className="mb-3">
          <div className="form-label">Match policies to existing clients by</div>
          <select
            className="form-select"
            style={{ maxWidth: 360 }}
            value={flow.matchBy}
            onChange={(event) => flow.setMatchBy(event.target.value as typeof flow.matchBy)}
          >
            <option value="ClientCode">Client code (or ClientExternalId)</option>
            <option value="NameAndPhone">Client name + phone</option>
          </select>
        </div>
      )}
      <div className="d-flex flex-wrap gap-2 align-items-center">
        <input
          className="form-control"
          style={{ maxWidth: 420 }}
          type="file"
          accept=".csv,.xlsx"
          onChange={(event) => flow.setFile(event.target.files?.[0] ?? null)}
        />
        <button className="btn btn-gold" type="button" disabled={flow.busy || !flow.file} onClick={() => void flow.runPreview()}>
          {flow.busy ? (
            <>
              <span className="spinner-border spinner-border-sm me-2" role="status" aria-hidden="true" />
              Reading file…
            </>
          ) : (
            `Preview ${noun}`
          )}
        </button>
        <button className="btn btn-outline-secondary" type="button" onClick={() => void onDownloadTemplate()}>
          Download template
        </button>
      </div>
      {flow.file && <p className="mt-3 mb-0 text-muted">{flow.file.name}</p>}
    </section>
  )
}

function PreviewStep({
  kind,
  flow,
  onConfirm,
  noun,
}: {
  kind: ImportKind
  flow: ReturnType<typeof useImportFlow>
  onConfirm: () => void
  noun: string
}) {
  const preview = flow.preview
  if (!preview) {
    return null
  }

  return (
    <section className="content-card">
      <div className="d-flex justify-content-between align-items-start flex-wrap gap-2 mb-3">
        <div>
          <h3 className="h5 mb-1">2. Review {preview.totalRows} rows</h3>
          <p className="mb-0 text-muted">
            {preview.validCount} valid, {preview.invalidCount} with errors. Invalid rows are highlighted and will be
            skipped.
          </p>
        </div>
        <button className="btn btn-link" type="button" onClick={flow.reset}>
          Choose a different file
        </button>
      </div>
      <div className="table-responsive import-preview-table">
        {kind === 'clients' ? <ClientPreviewTable rows={preview.rows} /> : <PolicyPreviewTable rows={preview.rows} />}
      </div>
      <div className="mt-3 d-flex gap-2">
        <button
          className="btn btn-gold"
          type="button"
          disabled={flow.busy || preview.validCount === 0}
          onClick={() => void onConfirm()}
        >
          {flow.busy ? (
            <>
              <span className="spinner-border spinner-border-sm me-2" role="status" aria-hidden="true" />
              Importing…
            </>
          ) : (
            `Import ${preview.validCount} valid ${noun}`
          )}
        </button>
      </div>
    </section>
  )
}

function ImportSummary({
  kind,
  imported,
  skipped,
  reasons,
  onAgain,
}: {
  kind: ImportKind
  imported: number
  skipped: number
  reasons: { rowNumber: number; reason: string }[]
  onAgain: () => void
}) {
  const noun = kind === 'clients' ? 'clients' : 'policies'
  const listPath = kind === 'clients' ? '/clients' : '/policies'
  return (
    <section className="content-card">
      <h3 className="h5">3. Import finished</h3>
      <p className="mb-3">
        <strong>
          {imported} {noun} imported, {skipped} skipped
        </strong>
        .
      </p>
      {reasons.length > 0 && (
        <ul className="text-muted">
          {reasons.slice(0, 20).map((item) => (
            <li key={`${item.rowNumber}-${item.reason}`}>
              Row {item.rowNumber}: {item.reason}
            </li>
          ))}
          {reasons.length > 20 && <li>…and {reasons.length - 20} more</li>}
        </ul>
      )}
      <div className="d-flex gap-2">
        <Link className="btn btn-gold" to={listPath}>
          Back to {noun}
        </Link>
        <button className="btn btn-outline-secondary" type="button" onClick={onAgain}>
          Import another file
        </button>
      </div>
    </section>
  )
}

function ClientPreviewTable({
  rows,
}: {
  rows: { rowNumber: number; isValid: boolean; error: string | null; values: ClientImportValues | PolicyImportValues }[]
}) {
  return (
    <table className="table table-sm align-middle">
      <thead>
        <tr>
          <th>Row</th>
          <th>Code</th>
          <th>Name</th>
          <th>Type</th>
          <th>Phone</th>
          <th>Email</th>
          <th>Status</th>
        </tr>
      </thead>
      <tbody>
        {rows.map((row) => {
          const values = row.values as ClientImportValues
          return (
            <tr key={row.rowNumber} className={row.isValid ? undefined : 'import-row-invalid'}>
              <td>{row.rowNumber}</td>
              <td>{values.clientCode}</td>
              <td>{values.companyName}</td>
              <td>{values.clientType}</td>
              <td>{values.phone}</td>
              <td>{values.email}</td>
              <td>{row.isValid ? 'Valid' : row.error}</td>
            </tr>
          )
        })}
      </tbody>
    </table>
  )
}

function PolicyPreviewTable({
  rows,
}: {
  rows: { rowNumber: number; isValid: boolean; error: string | null; values: ClientImportValues | PolicyImportValues }[]
}) {
  return (
    <table className="table table-sm align-middle">
      <thead>
        <tr>
          <th>Row</th>
          <th>Policy</th>
          <th>Client</th>
          <th>Insurer</th>
          <th>Type</th>
          <th>Start</th>
          <th>Expiry</th>
          <th>Premium</th>
          <th>Status</th>
        </tr>
      </thead>
      <tbody>
        {rows.map((row) => {
          const values = row.values as PolicyImportValues
          return (
            <tr key={row.rowNumber} className={row.isValid ? undefined : 'import-row-invalid'}>
              <td>{row.rowNumber}</td>
              <td>{values.policyNumber}</td>
              <td>{values.matchedClientName ?? values.clientName ?? values.clientCode}</td>
              <td>{values.insurer}</td>
              <td>{values.policyType}</td>
              <td>{values.startDate}</td>
              <td>{values.expiryDate}</td>
              <td>{values.premium}</td>
              <td>{row.isValid ? 'Valid' : row.error}</td>
            </tr>
          )
        })}
      </tbody>
    </table>
  )
}
