function CalculateCommand(cycle)
    --for k, v in ipairs(GetIdleAssets())
      --do print(v->ID())
    --end

    --[[goldAmount = GetGold() -- Call GetGold wrapper function for Gold()
    print("GOLD:")
    print(goldAmount)
    SetGold(goldAmount+1000)
    lumberAmount = GetLumber()
    print("LUMBER:")
    print(lumberAmount)
    SetLumber(lumberAmount+1000)--]]
    --[[if (cycle % 4 ~= 0) then
        return
    end --]]

    if(0 == GetFoundAssetCount(AssetType.atGoldMine)) then
--print("nogoldmine")
        --print("Trying SearchMap")
        PeasantSearchMap()
       --print("SearchMap")
    elseif((0 == GetPlayerAssetCount(AssetType.atTownHall)) and
            (0 == GetPlayerAssetCount(AssetType.atKeep))     and
            (0 == GetPlayerAssetCount(AssetType.atCastle))) then
        --print("Trying BuildTownHall")
--print("buildtownhall")
        BuildTownHall()
        --print("BuildTownHall")
    elseif(5 > GetPlayerAssetCount(AssetType.atPeasant)) then
        --print("Trying ActivatePeasants")
        ActivatePeasants(true)
        --print("ActivatePeasants")
    elseif(12 > GetSeenPercent()) then
        --print("Trying SearchMap")
        PeasantSearchMap()
        --print("SearchMap")
    else
        completedAction = 0
        footmanCount = GetPlayerAssetCount(AssetType.atFootman)
        archerCount = GetPlayerAssetCount(AssetType.atArcher) + GetPlayerAssetCount(AssetType.atRanger)
    if(0 == completedAction) then
        completedAction = Repair()
    end
        if(0 == completedAction and GetFoodConsumption() >= GetFoodProduction()) then
            --print("Trying BuildFarm")
            completedAction = BuildBuilding(AssetType.atFarm, AssetType.atFarm)
            if (0 ~= completedAction) then
                --print("BuildFarm")
            end
        end
        if(0 == completedAction) then
            --print("Trying ActivatePeasants")
            completedAction = ActivatePeasants(false)
            if (0 ~= completedAction) then
                --print("ActivatePeasants")
            end
        end
        if(0 == completedAction and 0 == GetPlayerAssetCount(AssetType.atBarracks)) then
            --print("Trying BuildBarracks")
            completedAction = BuildBuilding(AssetType.atBarracks, AssetType.atFarm)
            if (0 ~= completedAction) then
                --print("BuildBarracks")
            end
        end
        if(0 == completedAction and 5 > footmanCount) then
            --print("Trying TrainFootman")
            completedAction = TrainFootman()
            if (0 ~= completedAction) then
                --print("TrainFootman")
            end
        end
        if(0 == completedAction and 0 == GetPlayerAssetCount(AssetType.atLumberMill)) then
            --print("Trying BuildLumberMill")
            completedAction = BuildBuilding(AssetType.atLumberMill, AssetType.atBarracks)
            if (0 ~= completedAction) then
                --print("BuildLumberMill")
            end
        end
        if(0 == completedAction and 5 > archerCount) then
            --print("Trying TrainArcher")
            completedAction = TrainArcher()
            if (0 ~= completedAction) then
                --print("TrainArcher")
            end
        end
        if (0 == completedAction and 0 < GetPlayerAssetCount(AssetType.atFootman)) then
            --print("Trying FindEnemies")
            completedAction = FindEnemies()
            if (0 ~= completedAction) then
                --print("FindEnemies")
            end
        end
        if(0 == completedAction) then
            --print("Trying ActivateFighters")
            completedAction = ActivateFighters()
            if (0 ~= completedAction) then
                --print("ActivateFighters")
            end
        end
        if(0 == completedAction and (5 <= footmanCount) and (5 <= archerCount)) then
            --print("Trying AttackEnemies")
            completedAction = AttackEnemies()
            if (0 ~= completedAction) then
                --print("AttackEnemies")
            end
        end
        if(0 == completedAction and 0 == GetPlayerAssetCount(AssetType.atBlacksmith)) then
            --print("Trying BuildBlacksmith")
            completedAction = BuildBuilding(AssetType.atBlacksmith, AssetType.atLumberMill)
            if (0 ~= completedAction) then
                --print("BuildBlacksmith")
            end
        end
       
        if (0 == completedAction) then
            --print("None")
        end

        return completedAction
    end
end
