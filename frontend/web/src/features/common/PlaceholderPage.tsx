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
        <h3>Coming in a later release</h3>
        <p>{description}</p>
      </section>
    </div>
  )
}
