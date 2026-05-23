[HttpPost]
public async Task<IActionResult> CreateProduct(
    Product product
)
{
    _context.Products.Add(product);

    await _context.SaveChangesAsync();

    await _httpClient.PostAsJsonAsync(
        "http://localhost:5002/api/orders/sync",
        product
    );

    return Ok(product);
}