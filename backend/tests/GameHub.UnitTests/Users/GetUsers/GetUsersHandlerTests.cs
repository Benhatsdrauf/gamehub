using GameHub.Application.Common.Pagination;
using GameHub.Application.Users;
using GameHub.Application.Users.GetUsers;
using NSubstitute;

namespace GameHub.UnitTests.Users.GetUsers;

public class GetUsersHandlerTests
{
    private readonly IUserQueries _queries = Substitute.For<IUserQueries>();
    private readonly GetUsersHandler _sut;

    public GetUsersHandlerTests()
    {
        _sut = new GetUsersHandler(_queries);
    }

    // [Theory] + [InlineData] runs the SAME test body once per row — the parameterized
    // form (≈ test.each). Perfect for the paging-clamp guardrails: many inputs, one assertion.
    // Columns: inputPage, inputPageSize, expectedPage, expectedPageSize.
    [Theory]
    [InlineData(0, 20, 1, 20)]     // page below 1 clamps to 1
    [InlineData(-5, 20, 1, 20)]    // negative page clamps to 1
    [InlineData(1, 0, 1, 20)]      // pageSize below 1 falls back to default 20
    [InlineData(1, 500, 1, 100)]   // pageSize above the max clamps to 100
    [InlineData(3, 50, 3, 50)]     // in-range values pass through untouched
    public async Task Handle_ClampsPagingBeforeQuerying(
        int page, int pageSize, int expectedPage, int expectedPageSize)
    {
        // Arrange — the query side returns an (empty) page for whatever it's asked.
        _queries.GetUsersAsync(Arg.Any<int>(), Arg.Any<int>(), Arg.Any<CancellationToken>())
            .Returns(new PagedResponse<UserListItem>([], expectedPage, expectedPageSize, 0));

        // Act
        var result = await _sut.Handle(new GetUsersQuery(page, pageSize));

        // Assert — the handler queried with the CLAMPED values, not the raw client input.
        Assert.True(result.IsSuccess);
        await _queries.Received(1).GetUsersAsync(
            expectedPage, expectedPageSize, Arg.Any<CancellationToken>());
    }
}
