using Moq;
using Microsoft.AspNetCore.Http;
using WebApplication1.Services.NalogServices;
using WebApplication1.Services.NalogVozacAccessServices;
using WebApplication1.Repository.NalogRepository;
using WebApplication1.DataAccess.Models;


namespace WebApplication1.Tests.Services;

[TestFixture]
public class NalogServiceTests
{
    private Mock<INalogRepository> _repositoryMock;
    private Mock<ITureRepository> _turaRepositoryMock;
    private Mock<IHttpContextAccessor> _httpContextAccessorMock;
    private Mock<INalogVozacAccessService> _vozacAccessMock;
    private NalogService _service;

    [SetUp]
    public void SetUp()
    {
        _repositoryMock = new Mock<INalogRepository>();
        _turaRepositoryMock = new Mock<ITureRepository>();
        _httpContextAccessorMock = new Mock<IHttpContextAccessor>();
        _vozacAccessMock = new Mock<INalogVozacAccessService>();

        _service = new NalogService(
            _repositoryMock.Object,
            _turaRepositoryMock.Object,
            _httpContextAccessorMock.Object,
            _vozacAccessMock.Object);
    }

    [Test]
    public async Task CancelActiveInternalForTuraAsync_WhenNoNalog_ShouldReturnFalse()
    {
        _repositoryMock
            .Setup(r => r.GetActiveByTuraIdAsync(1))
            .ReturnsAsync((Nalog)null);

        var result = await _service.CancelActiveInternalForTuraAsync(1);

        Assert.That(result, Is.False);
    }

	[Test]
	public async Task CancelActiveInternalForTuraAsync_WhenNalogIsNotInternal_ShouldReturnFalse()
	{
		// Arrange
		var nalog = new Nalog
		{
			TuraId = 1,
			Prevoznik = new Prevoznik
			{
				Interni = false
			}
		};

		_repositoryMock
			.Setup(r => r.GetActiveByTuraIdAsync(1))
			.ReturnsAsync(nalog);

		// Act
		var result = await _service.CancelActiveInternalForTuraAsync(1);

		// Assert
		Assert.That(result, Is.False);
		_repositoryMock.Verify(r => r.Update(It.IsAny<Nalog>()), Times.Never);
	}

	[Test]
	public async Task CancelActiveInternalForTuraAsync_WhenNalogIsInternal_ShouldStornirajAndReturnTrue()
	{
		// Arrange
		var nalog = new Nalog
		{
			TuraId = 1,
			StatusNaloga = "U Toku",
			FinishedAt = DateTime.UtcNow,
			Prevoznik = new Prevoznik
			{
				Interni = true
			}
		};

		_repositoryMock
			.Setup(r => r.GetActiveByTuraIdAsync(1))
			.ReturnsAsync(nalog);

		// Act
		var result = await _service.CancelActiveInternalForTuraAsync(1);

		// Assert
		Assert.That(result, Is.True);
		Assert.That(nalog.StatusNaloga, Is.EqualTo("Storniran"));
		Assert.That(nalog.FinishedAt, Is.Null);

		_repositoryMock.Verify(r => r.Update(nalog), Times.Once);
	}
}