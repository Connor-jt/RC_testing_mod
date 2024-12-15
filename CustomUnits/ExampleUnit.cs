using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TestMod.CustomUnits{
    static class ExampleUnit{
        //public static 

        public static EntityBalancingParameters get_unit = new EntityBalancingParameters{
            entityId = "LightningTankFactory",
            prefabLocation = "Entities/Buildings/LightningTankFactory",
            imageLocation = "UnitUiImages/LightningTankFactory",
            rarity = Rarity.Common,
            gridSize = 2,
            factoryForEntityId = new NullableString { value = "LightningTank", hasValue = true },
            copyUpgradesToEntityId = new NullableString { value = "", hasValue = false },
            coinsAmount = 200,
            cost = 100,
            productionDuration = 0f,
            maxCapacity = 6,
            combatValue = 0,
            tech = Tech.Colorless,
            roles = UnitRole.Factory | UnitRole.Building,
            offeredSystemTags = SystemTags.None,
            maxHealth = 1500,
            maxShield = 0,
            maxMana = 0,
            maxRank = 0,
            maxArmor = 0f,
            armorProtection = 0f,
            moveSpeed = 0f,
            sightRadius = 15,
            attackType = 0,
            skillType = 0,
            damage1 = 0f,
            damage2 = 0f,
            weaponRange = 0f,
            attackCooldown = 0f,
            attack2Cooldown = 0f,
            effectRadius1 = 0f,
            effectRadius2 = 0f,
            skillRange = 0,
            healAmount1 = 0f,
            healAmount2 = 0f,
            gainCreditsAmount = 0f,
            skillManaCost = 0f,
            duration1 = 0f,
            duration2 = 0f,
            firePointCount = 0,
            neededExperienceLevel = 2,
            inactive = false,
            isAllowedForAi = false,
            isAllowedAsBlueprint = true,
            isAllowedAsStartingBlueprint = true,
            isAllowedForDemo = false,
            isForSpecialists = false,
        };
    }
}
