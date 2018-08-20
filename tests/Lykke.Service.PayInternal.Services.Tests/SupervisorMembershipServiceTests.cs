using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Common;
using Lykke.Logs;
using Lykke.Service.PayInternal.Core.Domain.Groups;
using Lykke.Service.PayInternal.Core.Domain.SupervisorMembership;
using Lykke.Service.PayInternal.Core.Services;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;

namespace Lykke.Service.PayInternal.Services.Tests
{
    [TestClass]
    public class SupervisorMembershipServiceTests
    {
        private Mock<ISupervisorMembershipRepository> _supervisorMembershipRepositoryMock;
        private Mock<IMerchantGroupService> _merchantGroupServiceMock;

        private ISupervisorMembershipService _supervisorMembershipService;

        public Mock<IMerchantGroupService> SetUpMerchantGroupService()
        {
            var groups = new List<IMerchantGroup>();

            var mock = new Mock<IMerchantGroupService>();

            mock.Setup(o => o.CreateAsync(It.IsAny<IMerchantGroup>()))
                .ReturnsAsync((IMerchantGroup m) => new MerchantGroup
                {
                    Id = Guid.NewGuid().ToString("D"),
                    Merchants = m.Merchants,
                    DisplayName = m.DisplayName,
                    MerchantGroupUse = m.MerchantGroupUse,
                    OwnerId = m.OwnerId
                })
                .Callback((IMerchantGroup m) => groups.Add(m));

            mock.Setup(o => o.GetByOwnerAsync(It.IsAny<string>()))
                .ReturnsAsync((string ownerId) => { return groups.Where(x => x.OwnerId == ownerId).ToList(); });

            return mock;
        }

        public Mock<ISupervisorMembershipRepository> SetUpMerchantMembershipRepository()
        {
            var memberships = new List<ISupervisorMembership>();

            var mock = new Mock<ISupervisorMembershipRepository>();

            mock.Setup(o => o.AddAsync(It.IsAny<ISupervisorMembership>()))
                .ReturnsAsync((ISupervisorMembership m) => m)
                .Callback((ISupervisorMembership m) => memberships.Add(m));

            mock.Setup(o => o.GetAsync(It.IsAny<string>()))
                .ReturnsAsync((string employeeId) =>
                {
                    return memberships.SingleOrDefault(x => x.EmployeeId == employeeId);
                });

            return mock;
        }

        [TestInitialize]
        public void TestInitialize()
        {
            _supervisorMembershipRepositoryMock = SetUpMerchantMembershipRepository();
            _merchantGroupServiceMock = SetUpMerchantGroupService();

            _supervisorMembershipService = new SupervisorMembershipService(
                _supervisorMembershipRepositoryMock.Object,
                EmptyLogFactory.Instance, 
                _merchantGroupServiceMock.Object);
        }

        [TestMethod]
        public async Task AddAsync_CreatesNewMerchantGroup()
        {
            const string employeeId = "SomeEmployeeId";

            const string merchantId = "SomeMerchantId";

            IMerchantsSupervisorMembership supervisorMembership = await _supervisorMembershipService.AddAsync(
                new MerchantsSupervisorMembership
                {
                    MerchantId = merchantId,
                    EmployeeId = employeeId,
                    Merchants = new List<string>
                    {
                        "merchant1",
                        "merchant2"
                    }
                });

            ISupervisorMembership membershipEntity =
                await _supervisorMembershipRepositoryMock.Object.GetAsync(employeeId);

            Assert.IsNotNull(supervisorMembership);
            Assert.IsNotNull(membershipEntity);
            Assert.IsTrue(membershipEntity.MerchantGroups.Count() == 1);
            Assert.IsTrue(membershipEntity.MerchantGroups.First().IsGuid());
            Assert.AreEqual(membershipEntity.EmployeeId, employeeId);
            Assert.AreEqual(membershipEntity.MerchantId, merchantId);

            IReadOnlyList<IMerchantGroup> merchantGroups =
                await _merchantGroupServiceMock.Object.GetByOwnerAsync(merchantId);

            IMerchantGroup merchantGroup = merchantGroups.Single();

            Assert.IsNotNull(merchantGroup);
            Assert.AreEqual(merchantGroup.MerchantGroupUse, MerchantGroupUse.Supervising);
            Assert.AreEqual(merchantGroup.OwnerId, merchantId);
            Assert.IsTrue(merchantGroup.Merchants.Length == 2);
        }
    }
}
