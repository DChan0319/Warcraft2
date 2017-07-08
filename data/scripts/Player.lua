functionPrint = 0
triggerPrint = 0

function RemoveAssetTrigger()
    if (triggerPrint == 1) then print("Footman count: ", GetPlayerAssetCount(AssetType.atFootman)) end
    Assets = GetAssets()
    if(3 < GetLivingPlayerAssetCount(AssetType.atFootman)) then

        for _, v in pairs(Assets) do
            if(GetType(v) == AssetType.atFootman) then
                if(GetAssetHealth(v) ~= 0) then
                    DamageAsset(v, 500)
                end
                break
            end
        end
    end
end

function CreateAssetTrigger()
if (functionPrint == 1) then print("CreateAssetTrigger()") end
    AddAsset(AssetType.atPeasant, 94, 58)
    NotifyTriggerEvent("CreateAssetTrigger", true)

end

function GoldExplorationTrigger()
    if(GetSeenPercent() > 10) then
        if (triggerPrint == 1) then print("You explored more than 10% of the map, here is a 100 gold bounty") end
        gold = GetGold()
        SetGold(gold + 100)
        NotifyTriggerEvent("GoldExplorationTrigger", true)
    end
end


function WoodExplorationTrigger()
    if( GetSeenPercent() > 10) then
        if (triggerPrint == 1) then print("You explored more than 10% of the map, here is a 100 lumber bounty") end
        wood = GetLumber()
        SetLumber(wood + 100)
        NotifyTriggerEvent("WoodExplorationTrigger", true)
    end
end

function GoldForFirstFootman()

SetGold(1000)
SetLumber(1000)
    if(GetPlayerAssetCount(AssetType.atFootman) > 0) then
        if (triggerPrint == 1) then print("First footman, good job, here is a 500 gold bounty") end
        gold = GetGold()
        SetGold(gold + 500)
        NotifyTriggerEvent("GoldForFirstFootman", true)
    end
end

function DamageIfWalkHere()
    --print(cycle)
    tilePosX = 528
    tilePosY = 144
    --TODO redo trigger manager and have tilePosX and tilePosY as parameters
    Assets = GetAssets()
    if(next(Assets) == nil) then
        return 0
    end
    for _, v in pairs(Assets) do
        tilePosOfAssetX, tilePosOfAssetY = GetAssetPosition(v)
        --print(tilePosOfAssetX)
        --print(tilePosOfAssetY)
        if((tilePosX == tilePosOfAssetX) and (tilePosY == tilePosOfAssetY)) then
            DamageAsset(v,1)
            break
        end
    end
end

function DamageTownHallEach100Cycles()
    if(cycle % 100 ~= 0) then
        return 0
    end
    Assets = GetAssets()
    if(next(Assets) == nil) then
        return 0
    end
    for _, v in pairs(Assets) do
        if(GetType(v) == AssetType.atTownHall) then
            DamageAsset(v,420)
            break
        end
    end
end

function LoseIfNoTownHallTrigger()

    if((cycle > 200) and GetPlayerAssetCount(AssetType.atTownHall) == 0) then
        print("YOU LOST YOU FOOL")
        NotifyTriggerEvent("LoseIfNoTownHallTrigger", true)
    end
end

function RegisterTriggers(cycle)
    -- register a trigger, state whether it can be called more than once, give a time between trigger calls
    --RegisterT("GoldExplorationTrigger", false)
    --RegisterT("WoodExplorationTrigger", false)
    --RegisterT("RemoveAssetTrigger", false)   
    --RegisterT("CreateAssetTrigger", false)
    --RegisterT("GoldForFirstFootman", false)
    --RegisterT("DamageIfWalkHere", false)
    --RegisterT("DamageTownHallEach100Cycles", false)
    --RegisterT("LoseIfNoTownHallTrigger", false)
end

--RegisterTriggers()
