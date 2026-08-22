import { useState } from 'react'
import { useQuery } from '@tanstack/react-query'
import { Form } from 'react-bootstrap'
import { Link } from 'react-router-dom'
import { fetchTasks, type TaskView } from '../../api/tasks'
import { fetchUsers } from '../../api/users'
import { CompleteTaskButton } from '../actions'
import { PriorityChip, StatusChip } from '../../components/display/StatusChips'
import { CustomSelect } from '../../components/display/CustomSelect'
import { EmptyState, ErrorBanner, LoadingBlock } from '../../components/feedback/PageFeedback'
import { formatIst, TASK_PRIORITIES, TASK_STATUSES } from './taskDisplay'

const TABS: { id: TaskView; label: string }[] = [
  { id: 'mine', label: 'My Tasks' },
  { id: 'team', label: 'Team Tasks' },
  { id: 'overdue', label: 'Overdue' },
  { id: 'completed', label: 'Completed' },
]

export function TasksPage() {
  const [view, setView] = useState<TaskView>('mine')
  const [status, setStatus] = useState('')
  const [priority, setPriority] = useState('')
  const [assignedUserPublicId, setAssignedUserPublicId] = useState('')
  const [fromDate, setFromDate] = useState('')
  const [toDate, setToDate] = useState('')

  const listQuery = useQuery({
    queryKey: ['tasks', view, status, priority, assignedUserPublicId, fromDate, toDate],
    queryFn: () =>
      fetchTasks({
        view,
        status: status || undefined,
        priority: priority || undefined,
        assignedUserPublicId: assignedUserPublicId || undefined,
        fromDate: fromDate || undefined,
        toDate: toDate || undefined,
        pageSize: 50,
      }),
  })

  const usersQuery = useQuery({ queryKey: ['users'], queryFn: fetchUsers })
  const tasks = listQuery.data?.items ?? []
  const users = usersQuery.data ?? []

  return (
    <div>
      <div className="page-heading">
        <h2>Tasks</h2>
        <p>Follow-ups and reminder work. Complete a task to stamp the time and write it on the timeline.</p>
      </div>

      <section className="content-card mb-3">
        <div className="filter-chips" role="tablist" aria-label="Task views">
          {TABS.map((tab) => (
            <button
              key={tab.id}
              type="button"
              role="tab"
              aria-selected={view === tab.id}
              className={`filter-chip${view === tab.id ? ' is-active' : ''}`}
              onClick={() => setView(tab.id)}
            >
              {tab.label}
            </button>
          ))}
        </div>
      </section>

      <section className="content-card mb-3">
        <div className="filter-bar filter-bar-tasks">
          <div className="filter-field">
            <label>Status</label>
            <CustomSelect
              value={status}
              onChange={setStatus}
              options={[
                { value: '', label: 'All statuses' },
                ...TASK_STATUSES.map((item) => ({
                  value: item,
                  label: item === 'InProgress' ? 'In progress' : item,
                })),
              ]}
            />
          </div>
          <div className="filter-field">
            <label>Priority</label>
            <CustomSelect
              value={priority}
              onChange={setPriority}
              options={[
                { value: '', label: 'All priorities' },
                ...TASK_PRIORITIES.map((item) => ({ value: item, label: item })),
              ]}
            />
          </div>
          <div className="filter-field">
            <label>Assigned Employee</label>
            <CustomSelect
              value={assignedUserPublicId}
              onChange={setAssignedUserPublicId}
              options={[
                { value: '', label: 'All employees' },
                ...users.map((user) => ({ value: user.publicId, label: user.fullName })),
              ]}
            />
          </div>
          <div className="filter-field">
            <label htmlFor="due-from">Due from</label>
            <Form.Control
              id="due-from"
              type="date"
              value={fromDate}
              onChange={(event) => setFromDate(event.target.value)}
            />
          </div>
          <div className="filter-field">
            <label htmlFor="due-to">Due to</label>
            <Form.Control
              id="due-to"
              type="date"
              value={toDate}
              onChange={(event) => setToDate(event.target.value)}
            />
          </div>
          <div className="filter-field align-self-end">
            <button
              type="button"
              className="btn btn-sm btn-outline-danger w-100"
              style={{ height: '38px' }}
              disabled={!status && !priority && !assignedUserPublicId && !fromDate && !toDate && view === 'mine'}
              onClick={() => {
                setView('mine')
                setStatus('')
                setPriority('')
                setAssignedUserPublicId('')
                setFromDate('')
                setToDate('')
              }}
              title="Reset all filters"
            >
              <i className="bi bi-arrow-counterclockwise me-1" /> Reset
            </button>
          </div>
        </div>
      </section>

      <section className="content-card">
        {listQuery.isError && <ErrorBanner>Could not load tasks. Check your connection and try again.</ErrorBanner>}
        {listQuery.isLoading && <LoadingBlock label="Loading tasks…" />}
        {!listQuery.isLoading && tasks.length === 0 && (
          <EmptyState icon="bi-check2-square" title="No tasks in this view" description="Follow-ups and milestone work will appear here when they are assigned." />
        )}
        {tasks.length > 0 && (
          <div className="table-responsive table-scroll">
            <table className="table align-middle mb-0">
              <thead>
                <tr>
                  <th>Task</th>
                  <th>Client</th>
                  <th>Policy</th>
                  <th>Due date</th>
                  <th>Priority</th>
                  <th>Assigned to</th>
                  <th>Status</th>
                  <th>Action</th>
                </tr>
              </thead>
              <tbody>
                {tasks.map((task) => (
                  <tr key={task.publicId} className={task.status === 'Overdue' ? 'row-attention' : undefined}>
                    <td>
                      <strong>{task.title}</strong>
                      {task.description && <div className="text-muted small">{task.description}</div>}
                    </td>
                    <td>{task.clientName ?? '—'}</td>
                    <td>{task.policyNumber ?? '—'}</td>
                    <td className={task.status === 'Overdue' ? 'is-due-now' : undefined}>{formatIst(task.dueDateUtc)}</td>
                    <td>
                      <PriorityChip priority={task.priority} />
                    </td>
                    <td>{task.assignedUserName ?? 'Unassigned'}</td>
                    <td>
                      <StatusChip status={task.status} />
                    </td>
                    <td>
                      <div className="table-actions">
                        <CompleteTaskButton publicId={task.publicId} status={task.status} />
                        <Link to={`/tasks/${task.publicId}`} className="btn btn-sm btn-action-view">
                          <i className="bi bi-eye me-1" />
                          View
                        </Link>
                      </div>
                    </td>
                  </tr>
                ))}
              </tbody>
            </table>
          </div>
        )}
      </section>
    </div>
  )
}
