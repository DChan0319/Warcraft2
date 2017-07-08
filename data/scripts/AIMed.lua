function CalculateCommand(cycle)
    if(0 == GetFoundAssetCount(AssetType.atGoldMine)) then
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
    if(0 == completedAction) then
        completedAction = Repair()
    end
        if(0 == completedAction and GetFoodConsumption() + 1 >= GetFoodProduction()) then
            completedAction = BuildBuilding(AssetType.atFarm, AssetType.atFarm)
        end
        if(0 == completedAction) then
            completedAction = ActivatePeasants(false)
        end
        if(0 == completedAction and 0 == GetPlayerAssetCount(AssetType.atBarracks)) then
            completedAction = BuildBuilding(AssetType.atBarracks, AssetType.atFarm)
        end
        if(0 == completedAction and 7 > footmanCount) then
            completedAction = TrainFootman()
        end
        if(0 == completedAction and 0 == GetPlayerAssetCount(AssetType.atLumberMill)) then
            completedAction = BuildBuilding(AssetType.atLumberMill, AssetType.atBarracks)
        end
        if(0 == completedAction and 7 > archerCount) then
            completedAction = TrainArcher()
        end
        if(0 == completedAction and Danger(500) and (footmanCount > 0 or archerCount > 0) and (aggression == 1)) then
           completedAction = AttackEnemies()
        end
        if (0 == completedAction and 0 < GetPlayerAssetCount(AssetType.atFootman)) then
            completedAction = FindEnemies()
        end
        if(0 == completedAction) then
            completedAction = ActivateFighters()
        end
        if(0 == completedAction and (7 <= footmanCount) and (7 <= archerCount)) then
            completedAction = AttackEnemies()
        end
        if(0 == completedAction and 0 == GetPlayerAssetCount(AssetType.atLumberMill)) then
            completedAction = BuildBuilding(AssetType.atLumberMill, AssetType.atBarracks)
        end
        if(0 == completedAction and 0 == GetPlayerAssetCount(AssetType.atBlacksmith)) then
            completedAction = BuildBuilding(AssetType.atBlacksmith, AssetType.atLumberMill)
        end
        if(0 == completedAction) then
            completedAction = UpgradeTownHall()
        end
        if(0 == completedAction and 1 == GetPlayerAssetCount(AssetType.atBlacksmith)) then
            completedAction = BlacksmithUpgrades()
        end

        return completedAction
    end
end
