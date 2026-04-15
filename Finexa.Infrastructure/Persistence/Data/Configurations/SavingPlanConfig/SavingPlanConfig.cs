//using System;
//using System.Collections.Generic;
//using System.Linq;
//using System.Text;
//using System.Threading.Tasks;
//using Finexa.Domain.Entities.Financial;
//using Microsoft.EntityFrameworkCore;
//using Microsoft.EntityFrameworkCore.Metadata.Builders;

//namespace Finexa.Infrastructure.Persistence.Data.Configurations.SavingPlanConfig
//{
//    public class SavingPlanConfig : BaseEntityConfig<SavingPlan>
//    {
//        public override void Configure(EntityTypeBuilder<SavingPlan> builder)
//        {
//            builder.Property(s => s.LimitAmount).HasColumnType("decimal(18,2)");

//            builder.Property(s => s.StartDate).IsRequired();
//            builder.Property(s => s.EndDate).IsRequired();
//        }
//    }
//}
