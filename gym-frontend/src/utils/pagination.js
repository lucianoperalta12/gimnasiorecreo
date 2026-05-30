function parsePositiveInteger(value) {
  const parsed = Number.parseInt(value, 10)
  return Number.isFinite(parsed) && parsed > 0 ? parsed : null
}

export function parsePaginatedResponse(response) {
  const items = Array.isArray(response?.data) ? response.data : []
  const totalCountHeader = response?.headers?.['x-total-count']
  const pageHeader = response?.headers?.['x-page']
  const pageSizeHeader = response?.headers?.['x-page-size']
  const activeCountHeader = response?.headers?.['x-active-count']
  const inactiveCountHeader = response?.headers?.['x-inactive-count']

  const totalCount = parsePositiveInteger(totalCountHeader) ?? items.length
  const page = parsePositiveInteger(pageHeader)
  const pageSize = parsePositiveInteger(pageSizeHeader)
  const activeCount = parsePositiveInteger(activeCountHeader)
  const inactiveCount = parsePositiveInteger(inactiveCountHeader)

  return {
    items,
    totalCount,
    page,
    pageSize,
    serverPaginationEnabled: page !== null && pageSize !== null,
    activeCount,
    inactiveCount
  }
}
