import { useState } from 'react'
import {
  confirmClientImport,
  confirmPolicyImport,
  previewClientImport,
  previewPolicyImport,
} from '../../api/import'
import type { ClientImportValues, ImportCommitResult, ImportPreview, PolicyImportValues } from '../../types/api'

export type ImportKind = 'clients' | 'policies'
export type ClientMatchBy = 'ClientCode' | 'NameAndPhone'

/**
 * Owns the three-step import wizard (upload → preview → confirm).
 * Use on the Clients and Policies import screens. Preview does not write to the database;
 * confirm sends the previewToken from step 2.
 */
export function useImportFlow(kind: ImportKind) {
  const [file, setFile] = useState<File | null>(null)
  const [matchBy, setMatchBy] = useState<ClientMatchBy>('ClientCode')
  const [preview, setPreview] = useState<ImportPreview<ClientImportValues | PolicyImportValues> | null>(null)
  const [result, setResult] = useState<ImportCommitResult | null>(null)
  const [busy, setBusy] = useState(false)
  const [error, setError] = useState<string | null>(null)

  async function runPreview() {
    if (!file) {
      setError('Choose a CSV or Excel file first.')
      return
    }

    setBusy(true)
    setError(null)
    setResult(null)
    try {
      const data =
        kind === 'clients' ? await previewClientImport(file) : await previewPolicyImport(file, matchBy)
      setPreview(data)
    } catch (caught) {
      setPreview(null)
      setError(caught instanceof Error ? caught.message : 'Preview failed.')
    } finally {
      setBusy(false)
    }
  }

  async function runConfirm() {
    if (!preview) {
      setError('Preview the file before importing.')
      return
    }

    setBusy(true)
    setError(null)
    try {
      const data =
        kind === 'clients'
          ? await confirmClientImport(preview.previewToken)
          : await confirmPolicyImport(preview.previewToken, matchBy)
      setResult(data)
    } catch (caught) {
      setError(caught instanceof Error ? caught.message : 'Import failed.')
    } finally {
      setBusy(false)
    }
  }

  function reset() {
    setFile(null)
    setPreview(null)
    setResult(null)
    setError(null)
    setBusy(false)
  }

  return {
    file,
    setFile,
    matchBy,
    setMatchBy,
    preview,
    result,
    busy,
    error,
    runPreview,
    runConfirm,
    reset,
  }
}
