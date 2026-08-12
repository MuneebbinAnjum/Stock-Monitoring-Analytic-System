// @ts-nocheck
// @vitest-environment jsdom
import React from 'react'
import { describe, it, expect, vi } from 'vitest'
import { render, screen, fireEvent, waitFor } from '@testing-library/react'
import userEvent from '@testing-library/user-event'
import AdminProducts from '../AdminProducts'

vi.mock('../../../api/product.api', () => ({
  ProductApi: {
    create: vi.fn(),
    update: vi.fn(),
    delete: vi.fn()
  }
}))

import { ProductApi } from '../../../api/product.api'

describe('AdminProducts component', () => {
  const categories = [{ id: '00000000-0000-0000-0000-000000000001', name: 'Phones' }]
  const products: any[] = []

  it('validates category GUID and shows server error on create failure', async () => {
    const onProductsUpdated = vi.fn()
    // make create reject with server-like response
    ;(ProductApi.create as any).mockRejectedValueOnce({ response: { data: { message: 'Category is required' } } })

    render(<AdminProducts products={products} categories={categories} onProductsUpdated={onProductsUpdated} />)

    // open modal
    const addBtn = screen.getByText(/add product/i)
    userEvent.click(addBtn)

    // fill required fields
    const nameInput = await screen.findByLabelText(/product name/i)
    const skuInput = screen.getByLabelText(/sku/i)
    const unitInput = screen.getByLabelText(/unit price/i)
    const stockInput = screen.getByLabelText(/stock quantity/i)
    const categorySelect = screen.getByLabelText(/category/i)

    await userEvent.type(nameInput, 'Test')
    await userEvent.type(skuInput, 'SKU-100')
    await userEvent.type(unitInput, '100')
    await userEvent.type(stockInput, '5')

    // intentionally select invalid category (empty) to trigger client-side validation
    await userEvent.selectOptions(categorySelect, [''])

    const submit = screen.getByRole('button', { name: /create product/i })
    userEvent.click(submit)

    // Because select invalid, should show client side message
    await waitFor(() => expect(screen.getByText(/name, sku, category, unit price, and stock are required/i)).toBeInTheDocument())

    // Now select valid category and submit; mock will reject with server message
    await userEvent.selectOptions(categorySelect, [categories[0].id])
    userEvent.click(submit)

    await waitFor(() => expect(ProductApi.create).toHaveBeenCalled())
    await waitFor(() => expect(screen.getByText(/category is required/i)).toBeInTheDocument())
  })
})
