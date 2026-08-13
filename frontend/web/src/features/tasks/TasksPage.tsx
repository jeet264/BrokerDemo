import { useState } from 'react'
import { useQuery } from '@tanstack/react-query'
import { Form } from 'react-bootstrap'
import { Link } from 'react-router-dom'
import { fetchTasks, type TaskView } from '../../api/tasks'
import { fetchUsers } from '../../api/users'
import { formatIst, priorityClass, TASK_PRIORITIES, TASK_STATUSES } from './taskDisplay'

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
          <Form.Select value={status} onChange={(event) => setStatus(event.target.value)} aria-label="Status">
            <option value="">All statuses</option>
            {TASK_STATUSES.map((item) => (
              <option key={item} value={item}>
                {item}
              </option>
            ))}
          </Form.Select>
          <Form.Select value={priority} onChange={(event) => setPriority(event.target.value)} aria-label="Priority">
            <option value="">All priorities</option>
            {TASK_PRIORITIES.map((item) => (
              <option key={item} value={item}>
                {item}
              </option>
            ))}
          </Form.Select>
          <Form.Select
            value={assignedUserPublicId}
            onChange={(event) => setAssignedUserPublicId(event.target.value)}
            aria-label="Assigned user"
          >
            <option value="">All employees</option>
            {users.map((user) => (
              <option key={user.publicId} value={user.publicId}>
                {user.fullName}
              </option>
            ))}
          </Form.Select>
          <Form.Control
            type="date"
            title="Due from"
            aria-label="Due from"
            value={fromDate}
            onChange={(event) => setFromDate(event.target.value)}
          />
          <Form.Control
            type="date"
            title="Due to"
            aria-label="Due to"
            value={toDate}
            onChange={(event) => setToDate(event.target.value)}
          />
        </div>
      </section>

      <section className="content-card">
        {listQuery.isError && <div className="alert alert-danger">Could not load tasks. Sign in and confirm the API is running.</div>}
        {listQuery.isLoading && <p className="text-muted mb-0">Loading tasks…</p>}
        {!listQuery.isLoading && tasks.length === 0 && <p className="text-muted mb-0">No tasks in this view.</p>}
        {tasks.length > 0 && (
          <div className="table-responsive">
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
                      <span className={priorityClass(task.priority)}>{task.priority}</span>
                    </td>
                    <td>{task.assignedUserName ?? 'Unassigned'}</td>
                    <td>{task.status}</td>
                    <td>
                      <div className="table-actions">
                        <Link to={`/tasks/${task.publicId}`} className="btn btn-sm btn-outline-secondary">
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
