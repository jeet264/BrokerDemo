/**
 * Empty-state page used for modules that exist in the nav but are not implemented yet
 * (clients UI, policies, renewals, tasks, activity, insurers UI, notifications, team).
 *
 * Props: title (heading), description (one-line purpose). No data fetching.
 */
export function PlaceholderPage({ title, description }: { title: string; description: string }) {
  return (
    <div>
      <div className="page-heading">
        <div>
          <h2>{title}</h2>
          <p>{description}</p>
        </div>
      </div>
      <section className="content-card empty-state">
        <i className="bi bi-inbox" />
        <h3>Nothing to show yet</h3>
        <p>This area is part of the workspace shell and will be filled when the {title.toLowerCase()} module is implemented.</p>
      </section>
    </div>
  )
}
