functionPrint = 0
searchMapPrint = 0
functionBodyPrint = 0
aggression = 0  --aggression on if 1 (default)

AssetType = {
    atNone        = 0,
    atWall        = 1,
    atPeasant     = 2,
    atFootman     = 3,
    atKnight      = 4,
    atArcher      = 5,
    atRanger      = 6,
    atGoldMine    = 7,
    atTownHall    = 8,
    atKeep        = 9,
    atCastle      = 10,
    atFarm        = 11,
    atBarracks    = 12,
    atLumberMill  = 13,
    atBlacksmith  = 14,
    atScoutTower  = 15,
    atGuardTower  = 16,
    atCannonTower = 17,
    atMax         = 18
}

CapabilityType = {
    actNone             = 0,
    actBuildPeasant     = 1,
    actBuildFootman     = 2,
    actBuildKnight      = 3,
    actBuildArcher      = 4,
    actBuildRanger      = 5,
    actBuildFarm        = 6,
    actBuildTownHall    = 7,
    actBuildBarracks    = 8,
    actBuildLumberMill  = 9,
    actBuildBlacksmith  = 10,
    actBuildKeep        = 11,
    actBuildCastle      = 12,
    actBuildScoutTower  = 13,
    actBuildGuardTower  = 14,
    actBuildCannonTower = 15,
    actMove             = 16,
    actRepair           = 17,
    actMine             = 18,
    actBuildSimple      = 19,
    actBuildAdvanced    = 20,
    actConvey           = 21,
    actShelter          = 22,
    actUnshelter        = 23,
    actCancel           = 24,
    actBuildWall        = 25,
    actAttack           = 26,
    actStandGround      = 27,
    actPatrol           = 28,
    actWeaponUpgrade1   = 29,
    actWeaponUpgrade2   = 30,
    actWeaponUpgrade3   = 31,
    actArrowUpgrade1    = 32,
    actArrowUpgrade2    = 33,
    actArrowUpgrade3    = 34,
    actArmorUpgrade1    = 35,
    actArmorUpgrade2    = 36,
    actArmorUpgrade3    = 37,
    actLongbow          = 38,
    actRangerScouting   = 39,
    actMarksmanship     = 40,
    actMax              = 41
}

AssetAction = {
    None          = 0,
    Construct     = 1,
    Build         = 2,
    Repair        = 3,
    Walk          = 4,
    StandGround   = 5,
    Shelter       = 6,
    Attack        = 7,
    MineGold      = 8,
    HarvestLumber = 9,
    QuarryStone   = 10,
    ConveyGold    = 11,
    ConveyLumber  = 12,
    ConveyStone   = 13,
    Death         = 14,
    Decay         = 15,
    Capability    = 16
}

TileType = {
    None           = 0,
    Grass          = 1,
    Dirt           = 2,
    Rock           = 3,
    Tree           = 4,
    Stump          = 5,
    Seedling       = 6,
    AdolescentTree = 7,
    Water          = 8,
    Wall           = 9,
    WallDamaged    = 10,
    Rubble         = 11,
    Max            = 12
}

PositionFlag = {
    FromAsset = 1,
    FromTile  = 2
}

--check for an asset then can move, if found, move it to closest unknown tile
function SearchMap()
    if(functionPrint == 1) then print("SearchMap()") end
    MovableIdle = GetMovableIdleAssets()

    -- get first asset with speed > 0
    for k, asset in pairs(MovableIdle) do
        if(GetAssetSpeed(asset) ~= 0) then
            MovableAsset = asset
            break
        end
    end

    -- create a command that sends actor to nearest unknown tile type
    if(MovableAsset) then --[[
        assetX, assetY = GetAssetPosition(MovableAsset)
        unknownPositionX, unknownPositionY = FindNearestReachableTileType(assetX, assetY, TileType.None)
        if (searchMapPrint == 1) then print("SearchMap",  unknownPositionX,  unknownPositionY) end
        if(0 <= unknownPositionX) then
            AddActor(MovableAsset)
            return TryCommand(PositionFlag.FromTile, CapabilityType.actMove, unknownPositionX, unknownPositionY)
        end --]]
    return Scout(MovableAsset)
    end
    return 0
end

--Uses only peasants to search map but assigns them a military escort
function PeasantSearchMap()
    if(functionPrint == 1) then print("PeasantSearchMap()") end
    MovableIdle = GetMovableIdleAssets()
    for k, asset in pairs(MovableIdle) do
    if(GetAssetSpeed(asset) ~= 0 and GetType(asset) == AssetType.atPeasant) then
            peasantAsset = asset
        break
        end
    end

    if(peasantAsset) then
    escortAsset = GetNearestMilitaryAsset(peasantAsset)
    if(-1 ~= escortAsset) then
        AddActor(escortAsset)
    end
    AddActor(peasantAsset)
    assetX, assetY = GetAssetPosition(peasantAsset)
        unknownPositionX, unknownPositionY = FindNearestReachableTileType(assetX, assetY, TileType.None)
        if (searchMapPrint == 1) then print("PeasantSearchMap",  unknownPositionX,  unknownPositionY) end
        if(0 <= unknownPositionX) then
        return TryCommand(PositionFlag.FromTile, CapabilityType.actMove, unknownPositionX, unknownPositionY)
    end
    end
    return 0
end

--Same as SearchMap but with a specific asset
function Scout(assetID)
    if(functionPrint == 1) then print("Scout()") end
    assetX, assetY = GetAssetPosition(assetID)
    unknownPositionX, unknownPositionY = FindNearestReachableTileType(assetX, assetY, TileType.None)
    if(searchMapPrint == 1) then print("Scout",  unknownPositionX,  unknownPositionY) end
    if(0 <= unknownPositionX) then
    AddActor(assetID)
    return TryCommand(PositionFlag.FromTile, CapabilityType.actMove, unknownPositionX, unknownPositionY)
    end
    return 0
end


-- Builds a town hall
function BuildTownHall()
    if (functionPrint == 1) then print("BuildTownHall()") end
    builderTable = GetAssetsWithCapability(CapabilityType.actBuildTownHall)
    if (functionBodyPrint == 1) then print("table LUA", builderTable) end
    if next(builderTable) == nil then
        return 0
    end
    
    builderX, builderY = GetAssetPosition(builderTable[0])
    if (functionBodyPrint == 1) then print("builderX", builderX, "builderY", builderY) end
    goldMine = GetNearestAsset(builderX, builderY, AssetType.atGoldMine)
    goldMineX, goldMineY = GetAssetPosition(goldMine)
    if (functionBodyPrint == 1) then print("goldMineX", goldMineX, "goldMineY", goldMineY) end
    builder = GetNearestPeasant(goldMineX, goldMineY)
    if(builder ~= -1) then
    placementX, placementY = GetBestAssetPlacement(goldMineX, goldMineY, builder, AssetType.atTownHall)
    if (functionBodyPrint == 1) then print("placementX", placementX, "placementYTownHall", placementY) end
    if(0 <= placementX) then
        AddActor(builder)
        return TryCommand(PositionFlag.FromTile, CapabilityType.actBuildTownHall, placementX, placementY)
    else
        return SearchMap()
    end
    end
    return 0
end

-- Builds a building of specified type, near the first of the other specified type
function  BuildBuilding(BuildingType, NearType)
    if(functionPrint == 1) then print("buildBuilding()") end
    Assets = GetAssets()
    buildType = {[AssetType.atBarracks]   = CapabilityType.actBuildBarracks,
                 [AssetType.atLumberMill] = CapabilityType.actBuildLumberMill,
                 [AssetType.atBlacksmith] = CapabilityType.actBuildBlacksmith,
         [AssetType.atScoutTower] = CapabilityType.actBuildScoutTower
                }
    buildAction = buildType[BuildingType] or CapabilityType.actBuildFarm
    builders = GetAssetsWithCapability(buildAction)
    townHall = GetAssetsWithCapability(CapabilityType.actBuildPeasant)[0]
    builder = nil
    nearAsset = nil
    assetIsIdle = false

    for _, asset in pairs(builders) do

        if(0 ~= AssetHasActiveCapability(asset, buildAction)) then
            return 0
        end
        if(0 ~= IsInterruptible(asset)) then
            if(not builder or (not assetIsIdle or GetCurAction(asset) == AssetAction.None)) then
                if (functionBodyPrint == 1) then print("Idle found: ", asset) end
                builder = asset
                assetIsIdle = GetCurAction(asset) == AssetAction.None
            end
        end
    end
    if(not(builder)) then
        if (functionBodyPrint == 1) then print("no builder found") end
        return 0
    end

    for _, asset in pairs(Assets) do
        if(GetType(asset) == NearType and GetCurAction(asset) ~= AssetAction.Construct) then
            nearAsset = asset
        end
        if(GetType(asset) == BuildingType and GetCurAction(asset) == AssetAction.Construct) then
            return 0
        end
    end

    if(BuildingType ~= NearType and not(nearAsset)) then
        return 0
    end

    builderX, builderY = GetAssetPosition(builder)

    if(nearAsset) then
        nearX, nearY = GetAssetPosition(nearAsset)
    else
        nearX, nearY = GetAssetPosition(townHall)
    end

    shiftedX, shiftedY = ShiftPosToCenter(nearX, nearY, builder, BuildingType, townHall)
    placementX, placementY = GetBestAssetPlacement(shiftedX, shiftedY, builder, BuildingType)

    if(0 > placementX) then
        return SearchMap()
    end

    if(GetCanInitiate(builder, buildAction)) then
        if(0 <= placementX) then
            if (functionBodyPrint == 1) then print("----Found a placement-----") print("PlacementX: ", placementX, "\nPlacementY: ", placementY) end
            AddActor(builder)
            return TryCommand(PositionFlag.FromTile, buildAction, placementX, placementY)
        end
    end

    return 0
end

-- Keeps peasants busy, gathering resources they should, and creating more if neccasary
function ActivatePeasants(trainmore)
    if (functionPrint == 1) then print("ActivatePeasants()") end
    MiningAssets       = GetAssetsWithCapability(CapabilityType.actMine)
    GoldMiners         = 0
    LumberHarvesters   = 0
    StoneHarvesters    = 0
    CarriedLumber      = 0
    CarriedGold        = 0
    CarriedStone       = 0
    MiningAsset        = nil
    TownHallAsset      = nil
    InterruptibleAsset = nil
    SwitchToLumber     = false
    SwitchToGold       = false
    SwitchToStone      = false

    -- get MiningAsset and an InterruptibleAsset
    for k, Asset in pairs(MiningAssets) do
        if(GetAssetSpeed(Asset) > 0) then
            -- --print("GoldMiners", GoldMiners)
            if((not MiningAsset) and (AssetAction.None == GetCurAction(Asset))) then
        if (functionBodyPrint == 1) then print("making idle -> mining") end
                MiningAsset = Asset
            end

            if(AssetHasAction(Asset, AssetAction.MineGold) == 1) then
                GoldMiners = GoldMiners + 1
                if(IsInterruptible(Asset) and (AssetAction.None ~= GetCurAction(Asset))) then
                    InterruptibleAsset = Asset
                end
            elseif(AssetHasAction(Asset, AssetAction.HarvestLumber) == 1) then
                LumberHarvesters = LumberHarvesters + 1
                if(IsInterruptible(Asset) and (AssetAction.None ~= GetCurAction(Asset))) then
                    InterruptibleAsset = Asset
                end
            elseif(AssetHasAction(Asset, AssetAction.HarvestStone) == 1) then
                StoneHarvesters = StoneHarvesters + 1
                if(IsInterruptible(Asset) and (AssetAction.None ~= GetCurAction(Asset))) then
                    InterruptibleAsset = Asset
                end
            end
        end
    end

    if (functionBodyPrint == 1) then print("# of gold miners: ", GoldMiners) print("# of lumber harvesters: ", LumberHarvesters) print("# of stone harvesters: ", StoneHarvesters)end

    BuildPeasantAssets = GetAssetsWithCapability(CapabilityType.actBuildPeasant)
    for k, Asset in pairs(BuildPeasantAssets) do
        if(AssetAction.None == GetCurAction(Asset)) then
            TownHallAsset = Asset
            break
        end
    end

    -- decide if more wood is required or more gold
    if    ((0 == LumberHarvesters) and (2 <= GoldMiners)) then
        SwitchToLumber = true
    elseif((2 <= LumberHarvesters) and (0 == GoldMiners)) then
        SwitchToGold   = true
    elseif((2 <= LumberHarvesters) and (2 <= GoldMiners) and (1 >= StoneHarvesters)) then
        SwitchToStone  = true
    end

    if(MiningAsset) then
        CarriedLumber = GetAssetGold  (MiningAsset)
        CarriedGold   = GetAssetLumber(MiningAsset)
        CarriedStone  = GetAssetStone (MiningAsset)
    end

    if(MiningAsset or (InterruptibleAsset and (SwitchToLumber or SwitchToGold or SwitchToStone))) then
        if(MiningAsset and (CarriedLumber > 0 or CarriedGold > 0 or CarriedStone > 0)) then
            if (functionBodyPrint == 1) then print("########## RETURNING GOLD/WOOD/STONE ##################") end
            AddActor(MiningAsset)
            townHallX, townHallY = GetAssetPosition(TownHallAsset)
            return TryCommand(PositionFlag.FromAsset, CapabilityType.actConvey, townHallX, townHallY, GetType(TownHallAsset), GetColor(TownHallAsset))
        else
            if(not MiningAsset) then
                MiningAsset = InterruptibleAsset
            end
        end

        MinerX, MinerY =  GetAssetPosition(MiningAsset)
        if((GoldMiners > 0 or StoneHarvesters > 0) and ((GetGold() > GetLumber() * 3) or (GetStone() > GetLumber()) or SwitchToLumber)) then
            LumberLocationX, LumberLocationY = FindNearestReachableTileType(MinerX, MinerY, TileType.Tree)

            if(LumberLocationX >= 0 and (AssetHasAction(MiningAsset, AssetAction.HarvestLumber) == 0)) then
                AddActor(MiningAsset)
                if (functionBodyPrint == 1) then print("########## SENDING PEASANT TO CHOP WOOD #############!!!!!") end
                return TryCommand(PositionFlag.FromTile, CapabilityType.actMine, LumberLocationX, LumberLocationY)
            else
                if (functionBodyPrint == 1) then print("######### SEARCHING FOR WOOD ###################") end
                return PeasantSearchMap()
            end
        elseif ((GoldMiners > 0 or LumberHarvesters > 0) and ((GetGold() > GetStone() * 10) or SwitchToStone)) then
            StoneLocationX, StoneLocationY = FindNearestReachableTileType(MinerX, MinerY, TileType.Rock)
            if(StoneLocationX >= 0 and (AssetHasAction(MiningAsset, AssetAction.HarvestStone) == 0)) then
                AddActor(MiningAsset)
                if (functionBodyPrint == 1) then print("########## SENDING PEASANT TO CHOP rock #############!!!!!") end
                return TryCommand(PositionFlag.FromTile, CapabilityType.actMine, StoneLocationX, StoneLocationY)
            else
                if (functionBodyPrint == 1) then print("######### SEARCHING FOR rock ###################") end
                return PeasantSearchMap()
            end
        else
            goldMine = GetNearestAsset(MinerX, MinerY, AssetType.atGoldMine)
            GoldLocationX, GoldLocationY = GetAssetPosition(goldMine)
        if (functionBodyPrint == 1) then print(GoldLocationX, GoldLocationY) end
        if(GoldLocationX >= 0 and (AssetHasAction(MiningAsset, AssetAction.MineGold) == 0)) then
                AddActor(MiningAsset)
                if (functionBodyPrint == 1) then print("Gold X: ", GoldLocationX, "\nGold Y: ", GoldLocationY) print("Current Mining Asset#: ", MiningAsset) print("##################### SENDING PEASANT TO MINE ####################") end
                return TryCommand(PositionFlag.FromAsset, CapabilityType.actMine, GoldLocationX, GoldLocationY, AssetType.atGoldMine)
            else
                if (functionBodyPrint == 1) then print("######## SEARCHING FOR GOLD ############") end
                    return PeasantSearchMap()
            end
        end
        return 1
    elseif(TownHallAsset and trainmore) then
        if (functionBodyPrint == 1) then print("############### TRAINING A PEASANT ################") end
        if(GetCanApply(TownHallAsset, CapabilityType.actBuildPeasant) == 1) then
            AddActor(TownHallAsset)
            townHallX, townHallY = GetAssetPosition(TownHallAsset)
            return TryCommand(PositionFlag.FromAsset, CapabilityType.actBuildPeasant, townHallX, townHallY)
        end
    end
    return 0
end

-- Trains a footman
function TrainFootman()
    if (functionPrint == 1) then print("TrainFootman()") end
    BarracksAsset = nil

    BuildFootmanAssets = GetAssetsWithCapability(CapabilityType.actBuildFootman)
    for k, Asset in pairs(BuildFootmanAssets) do
        if(AssetAction.None == GetCurAction(Asset)) then
            BarracksAsset = Asset
            break
        end
    end

    if(BarracksAsset) then
        if(0 == AssetHasActiveCapability(BarracksAsset, CapabilityType.actBuildFootman) and
           GetCanApply(BarracksAsset, CapabilityType.actBuildFootman) == 1) then
            AddActor(BarracksAsset)
            barracksX, barracksY = GetAssetPosition(BarracksAsset)
            return TryCommand(PositionFlag.FromAsset, CapabilityType.actBuildFootman, barracksX, barracksY)
        end
    end
    return 0
end

-- Trains an Archer
function TrainArcher()
    if (functionPrint == 1) then print("TrainArcher()") end
    Asset = nil
    BuildType = nil

    IdleAssets = GetIdleAssets()
    for _, asset in pairs(IdleAssets) do
        if(AssetHasCapability(asset, CapabilityType.actBuildArcher) == 1) then
            Asset = asset
            BuildType = CapabilityType.actBuildArcher
            break
        end
        if(AssetHasCapability(asset, CapabilityType.actBuildRanger) == 1) then
            Asset = asset
            BuildType = CapabilityType.actBuildRanger
            break
        end
    end

    if(Asset) then
        if(GetCanApply(Asset, BuildType) == 1) then
            AddActor(Asset)
            assetX, assetY = GetAssetPosition(Asset)
            return TryCommand(PositionFlag.FromAsset, BuildType, assetX, assetY)
        end
    end
    return 0
end

-- Gives Fighters regular commands, aka just stand around
function ActivateFighters()
    if (functionPrint == 1) then print("ActivateFighters()") end
    MovableIdle = GetMovableIdleAssets()
    fighterAsset = nil

    if(next(MovableIdle) == nil) then
        return 0
    end

    for _, asset in pairs(MovableIdle) do
    if(IsMilitary(asset)) then
            if(0 == AssetHasAction(asset, AssetAction.StandGround) and
            0 == AssetHasActiveCapability(asset, CapabilityType.actStandGround)) then
        fighterAsset = asset
        AddActor(asset)
            end
        end
    end
    if(fighterAsset) then
        return TryCommand(PositionFlag.None, CapabilityType.actStandGround)
    end
    return 0
end

-- Tells this unt to look units not under your control
function FindEnemies()
    if (functionPrint == 1) then print("FindEnemies()") print("gotta find the bad guys innit ?") end
    townHallAsset = GetAssetsWithCapability(CapabilityType.actBuildPeasant)
    if(next(townHallAsset) == nil) then
        return 0
    end
    townHallX, townHallY = GetAssetPosition(townHallAsset[0])
    if(EnemiesNotDiscovered(townHallX, townHallY) == 1) then
        return SearchMap()
    end
if (functionBodyPrint == 1) then print("found enemies") end
    return 0
end

-- Attacks enemies
function AttackEnemies()
    if (functionPrint == 1) then print("AttackEnemies()") end
    if (aggression == 0) then return 0 end
    actorNumber = 0
    averageLocationX = 0
    averageLocationY = 0
    assets = GetAssets()
    if(assets == nil) then
        return 0
    end
    for _, asset in pairs(assets) do
        if(IsMilitary(asset)) then
            if AssetHasAction(asset, AssetAction.Attack ) ~= 1 then
                actorNumber = actorNumber + 1
                AddActor(asset)
                posX, posY = GetAssetPosition(asset)
                averageLocationX = averageLocationX + posX
                averageLocationY = averageLocationY + posY
            end
        end
    end

    if(actorNumber > 0) then
        if (functionBodyPrint == 1) then print("on a un soldat !", actorNumber) end
        averageLocationX = averageLocationX / actorNumber
        averageLocationY = averageLocationY / actorNumber
        targetEnemy = FindNearestEnemy(averageLocationX, averageLocationY)
        if(targetEnemy == -1) then --no enemies
            if (functionBodyPrint == 1) then print("pas d'enemies à l'horizon !") end
            ClearActors()
            if (functionBodyPrint == 1) then print("clerd qctur!") end
            return SearchMap()
        end
        targetEnemyX, targetEnemyY = GetAssetPosition(targetEnemy)
        return TryCommand(PositionFlag.FromAsset, CapabilityType.actAttack, targetEnemyX,
                          targetEnemyY, GetType(targetEnemy), GetColor(targetEnemy))--TODO finish
    end
end


-- Upgrades TownHall
function UpgradeTownHall()
    if (functionPrint == 1) then print("UpgradeTownHall()") end
    if(0 == GetPlayerAssetCount(AssetType.atCastle)) then
    keepAsset = nil
    townHallAsset = nil
    assets = GetIdleAssets()
    for _, asset in pairs(assets) do
        if(GetType(asset) == AssetType.atKeep) then
        keepAsset = asset
        break
        end
        if(GetType(asset) == AssetType.atTownHall) then
        townHallAsset = asset
        end
    end
    
    if(keepAsset) then
        AddActor(keepAsset)
        return TryCommand(PositionFlag.None, CapabilityType.actBuildCastle)
    elseif(townHallAsset) then
        AddActor(townHallAsset)
        return TryCommand(PositionFlag.None, CapabilityType.actBuildKeep)
    end
    end
    
    return 0
end
-- Upgrades Ranger
function RangerUpgrade()
    Asset = nil
    if (functionPrint == 1) then print("RangerUpgrade()") end
    IdleAssets = GetIdleAssets()
    for _, asset in pairs(IdleAssets) do
        if(AssetHasCapability(asset, CapabilityType.actBuildRanger) == 1) then
            Asset = asset
            break
        end
    end
    if(Asset) then
	if(AssetType.atLumberMill == GetType(Asset)) then
	    AddActor(Asset)
	    return TryCommand(PositionFlag.None, CapabilityType.actBuildRanger)
	end
    end
    return 0
end

-- Upgrades Blacksmith
function BlacksmithUpgrades()
    if (functionPrint == 1) then print("BlacksmithUpgrades()") end
    blacksmithAsset = nil

    IdleAssets = GetIdleAssets()
    for _, asset in pairs(IdleAssets) do
        if(AssetType.atBlacksmith == GetType(asset)) then
            blacksmithAsset = asset
            break
        end
    end

    upgradeTypes = {CapabilityType.actWeaponUpgrade1, CapabilityType.actWeaponUpgrade2, CapabilityType.actWeaponUpgrade3,
                    CapabilityType.actArmorUpgrade1, CapabilityType.actArmorUpgrade2, CapabilityType.actArmorUpgrade3}
    if(blacksmithAsset) then
        for _, upgrade in pairs(upgradeTypes) do
            if(1 == AssetHasCapability(blacksmithAsset, upgrade) and
               0 == AssetHasActiveCapability(blacksmithAsset, upgrade)) then
                if (functionBodyPrint == 1) then print("BLACKSMITH UPGRADE: ", upgrade) end
                AddActor(blacksmithAsset)
                return TryCommand(PositionFlag.None, upgrade)
            end
        end
    end
    return 0
end

-- Upgrades Lumbermill
function LumberMillUpgrades()
    if (functionPrint == 1) then print("LumberMillUpgrades()") end
    lumberMillAsset = nil

    IdleAssets = GetIdleAssets()
    for _, asset in pairs(IdleAssets) do
        if(AssetType.atLumberMill == GetType(asset)) then
            lumberMillAsset = asset
            break
        end
    end

    upgradeTypes = {CapabilityType.actArrowUpgrade1, CapabilityType.actArrowUpgrade2, CapabilityType.actArrowUpgrade3,
                    CapabilityType.actLongbow, CapabilityType.actRangerScouting, CapabilityType.actMarksmanship}
    if(lumberMillAsset) then
        for _, upgrade in pairs(upgradeTypes) do
            if(1 == AssetHasCapability(lumberMillAsset, upgrade) and
               0 == AssetHasActiveCapability(lumberMillAsset, upgrade)) then
                if (functionBodyPrint == 1) then print("LUMBERMILL UPGRADE: ", upgrade) end
                AddActor(lumberMillAsset)
                return TryCommand(PositionFlag.None, upgrade)
            end
        end
    end
    return 0
end

--[[
-- Sets health of a unit to specified value
function SetHealth(assetID, health)
    if (functionPrint == 1) then print("SetHealth()") end
    if (0 < GetAssetHealth(assetID)) then
    if(0 >= health) then
        DamageAsset(assetID, 9999) --Arbitrary large number
    else
        SetAssetHealth(assetID, health)
    end
    end
    return 0
end
]]--

--Same as GetPlayerAssetCount but avoids a problem with decaying assets
function GetLivingPlayerAssetCount(assetType)
    if (functionPrint == 1) then print("GetLivingPlayerAssetCount()") end
    count = 0
    Assets = GetAssets()
    if(next(Assets) == nil) then
        return 0
    end
    for _, v in pairs(Assets) do
        if(GetType(v) == assetType) then
        count = count + 1
    end
    end
    return count
end

--returns Euclidean distance of asset and given position
function AssetDistanceFrom(assetID, posX, posY)
    if (functionPrint == 1) then print("AssetDistanceFrom()") end
    assetPosX, assetPosY = GetAssetPosition(assetID)
    return math.sqrt(math.pow((assetPosX - posX), 2) + math.pow((assetPosY - posY), 2))
end

--Checks how far our town halls are from gold mines and builds a new one if needed
function TownHallCheck()
    if (functionPrint == 1) then print("TownHallCheck()") end
    minDist = 9999 --Arbitrary large number
    asset = nil
    THAssets = GetAssetsWithCapability(CapabilityType.actBuildPeasant)
    for k, Asset in pairs(THAssets) do
    xPos, yPos = GetAssetPosition(Asset)
    goldMine = GetNearestAsset(xPos, yPos, AssetType.atGoldMine)
    if(goldMine ~= -1) then
        GoldLocationX, GoldLocationY = GetAssetPosition(goldMine)
        if(GoldLocationX >= 0) then
        distance = AssetDistanceFrom(Asset, GoldLocationX, GoldLocationY)
        minDist = math.min(minDist, distance)
        end
    end
    end
    if(minDist > 400) then
    BuildTownHall()
    end
    return 0
end

function IsMilitary(assetID)
    if (functionPrint == 1) then print("IsMilitary()") end
    return (AssetType.atPeasant ~= GetType(assetID) and GetAssetSpeed(assetID) ~= 0)
end

function Danger(range)
    if (functionPrint == 1) then print("Danger()") end
    assets = GetAssets()
    minEnemyDist = 999999 --Arbitrary Large distance
    for k, Asset in pairs(assets) do
    posX, posY = GetAssetPosition(Asset)
    enemyAsset = FindNearestEnemy(posX, posY)
    if(enemyAsset ~= -1) then
        enemyX, enemyY = GetAssetPosition(enemyAsset)
        distFromEnemy = AssetDistanceFrom(Asset, enemyX, enemyY)
        if(distFromEnemy < minEnemyDist) then
        minEnemyDist = distFromEnemy
        end
    end
    end

    return (minEnemyDist < range)
end

function GoldMineTaken(goldMineID)
    if (functionPrint == 1) then print("GoldMineTaken()") end
    goldX, goldY = GetAssetPosition(goldMineID)
    enemyAsset = FindNearestEnemy(goldX, goldY)
    if(enemyAsset ~= -1) then
    enemyX, enemyY = GetAssetPosition(enemyAsset)
    distFromEnemy = AssetDistanceFrom(Asset, enemyX, enemyY)
    if(distFromEnemy < 500) then
        return 1
    end
    end
    return 0
end

--GetNearestAsset() can return enemy asset so we have this to find our peasant
function GetNearestPeasant(xPos, yPos)
    if (functionPrint == 1) then print("GetNearestPeasant()") end
    builderTable = GetAssetsWithCapability(CapabilityType.actBuildTownHall)
    if (functionBodyPrint == 1) then print("table LUA", builderTable) end
    closestPeasant = -1
    minDist = 99999 --Arbitrary large number
    for _, asset in pairs(builderTable) do
    if(0 ~= IsInterruptible(asset)) then
        dist = (AssetDistanceFrom(asset, xPos, yPos))
        if(dist < minDist) then
        closestPeasant = asset
        minDist = dist 
        end
    end
    end
    return closestPeasant
end

-- Repairs all buildings that are below 50% health
function Repair()
    assetTable = GetAssets()
    if (functionPrint == 1) then print("Repair()") end
    for _, asset in pairs(assetTable) do
    if(GetAssetSpeed(asset) == 0) then --Are buildings the only asset with 0 speed?
        if(GetAssetHealth(asset) <= GetAssetDamageTaken(asset) and GetCurAction(asset) ~= AssetAction.Construct) then
        assetX, assetY = GetAssetPosition(asset)
        peasant = GetNearestAsset(assetX, assetY, AssetType.atPeasant)
        if(peasant ~= -1 and (1 ~= AssetHasAction(peasant, AssetAction.Repair))) then
            AddActor(peasant)
            --pX, pY = GetAssetTilePosition(peasant)
            --print("Peasant pos: ", pX, pY)
            return TryCommand(PositionFlag.FromAsset, CapabilityType.actRepair, assetX, assetY, GetType(asset), GetColor(asset))
        end
        end
    end
    end
    return 0
end

--Upgrade Our Towers
function UpgradeTower()
    assetTable = GetIdleAssets()
    upgradeType = CapabilityType.actBuildGuardTower
    if(GetPlayerAssetCount(AssetType.atGuardTower) - 1 > (GetPlayerAssetCount(AssetType.atCannonTower))) then
    upgradeType = CapabilityType.actBuildCannonTower
    end
    if (functionPrint == 1) then print("UpgradeTower()") end
    for _, asset in pairs(assetTable) do
    if (GetType(asset) == AssetType.atScoutTower) then
        AddActor(asset)
        return TryCommand(PositionFlag.None, upgradeType)
    end
    end
    return 0
end

-- Returns the id of the nearest ally military asset or -1 if none found
function GetNearestMilitaryAsset(assetID)
    if (functionPrint == 1) then print("GetNearestMilitaryAsset()") end
    minDist = 99999 --Arbitary large number
    assetTable = GetAssets()
    
    for _, asset in pairs(assetTable) do
    if(IsMilitary(asset)) then
        assetPosX, assetPosY = GetAssetPosition(asset)
        dist = AssetDistanceFrom(assetID, assetPosX, assetPosY)
        
        if(dist < minDist) then
        closestAsset = asset
        minDist = dist
        end
    end
    end
    
    if(closestAsset) then
    return closestAsset
    end
    return -1
end
