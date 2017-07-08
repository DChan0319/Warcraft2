cheatingAI = 0

function CalculateCommand(cycle)
    if(functionBodyPrint == 1) then 
            print("Gold: ", GetGold())
            print("Lumber: ", GetLumber())
            print("cycle ", cycle)
    end
    if (cheatingAI) then   
        if (cycle % 250 == 0) then
            gold = GetGold()
            SetGold(gold + 1000)
            lumber = GetLumber()
            SetLumber(lumber + 1000)
        end
    end
    if(0 == GetFoundAssetCount(AssetType.atGoldMine) and 90 > GetSeenPercent()) then
        PeasantSearchMap()
    elseif((0 == GetPlayerAssetCount(AssetType.atTownHall)) and
            (0 == GetPlayerAssetCount(AssetType.atKeep))     and
            (0 == GetPlayerAssetCount(AssetType.atCastle))) then
        BuildTownHall()
    elseif(5 > GetPlayerAssetCount(AssetType.atPeasant)) then
        ActivatePeasants(true)
    elseif(12 > GetSeenPercent()) then
        PeasantSearchMap()
    else
        completedAction = 0
        footmanCount = GetPlayerAssetCount(AssetType.atFootman)
        archerCount = GetPlayerAssetCount(AssetType.atArcher) + GetPlayerAssetCount(AssetType.atRanger)
    towerCount = GetPlayerAssetCount(AssetType.atScoutTower) + GetPlayerAssetCount(AssetType.atGuardTower) + GetPlayerAssetCount(AssetType.atCannonTower)
    if(0 == completedAction) then
        completedAction = Repair()
    end
        if(0 == completedAction and GetFoodConsumption() + 2 >= GetFoodProduction()) then
            completedAction = BuildBuilding(AssetType.atFarm, AssetType.atFarm)
        end
        if(0 == completedAction) then
            completedAction = ActivatePeasants(false)
        end
        if(0 == completedAction and 2 > GetPlayerAssetCount(AssetType.atBarracks)) then
            completedAction = BuildBuilding(AssetType.atBarracks, AssetType.atFarm)
        end
        if(0 == completedAction and 10 > footmanCount) then
            completedAction = TrainFootman()
        end
        if(0 == completedAction and 0 == GetPlayerAssetCount(AssetType.atLumberMill)) then
            completedAction = BuildBuilding(AssetType.atLumberMill, AssetType.atBarracks)
        end
        if(0 == completedAction and 10 > archerCount) then
            completedAction = TrainArcher()
        end
        if(0 == completedAction and Danger(1000) and (footmanCount > 0 or archerCount > 0) and (aggression == 1)) then
           completedAction = AttackEnemies()
        end
        if (0 == completedAction and 0 < GetPlayerAssetCount(AssetType.atFootman)) then
            completedAction = FindEnemies()
        end
        if(0 == completedAction) then
            completedAction = ActivateFighters()
        end
        if(0 == completedAction and (10 <= footmanCount) and (10 <= archerCount)) then
            completedAction = AttackEnemies()
        end
        if(0 == completedAction and 0 == GetPlayerAssetCount(AssetType.atLumberMill)) then
            completedAction = BuildBuilding(AssetType.atLumberMill, AssetType.atBarracks)
        end
        if(0 == completedAction and 0 == GetPlayerAssetCount(AssetType.atBlacksmith)) then
            completedAction = BuildBuilding(AssetType.atBlacksmith, AssetType.atLumberMill)
        end
    if(0 == completedAction and 2 > towerCount) then 
        completedAction = BuildBuilding(AssetType.atScoutTower, AssetType.atScoutTower) --Where do we want our towers?
    end
    if(0 == completedAction and 0 < GetPlayerAssetCount(AssetType.atScoutTower)) then
        completedAction = UpgradeTower()
    end
    if(0 == completedAction and (0 == GetPlayerAssetCount(AssetType.atCastle))) then
           completedAction = UpgradeTownHall()
    end
        if(0 == completedAction) then
           completedAction = RangerUpgrade()
        end
        if(0 == completedAction and 1 == GetPlayerAssetCount(AssetType.atBlacksmith)) then
            completedAction = BlacksmithUpgrades()
        end
        if(0 == completedAction and 1 == GetPlayerAssetCount(AssetType.atLumberMill)) then
            completedAction = LumberMillUpgrades()
        end
    if(0 == completedAction and 8 > GetPlayerAssetCount(AssetType.atPeasant)) then
        ActivatePeasants(true)
    end
    if(0 == completedAction) then
       completedAction = TownHallCheck()
    end

        return completedAction
    end
end
