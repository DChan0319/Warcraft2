-- Triggers.lua

function TimeTriggerCheck(time)
    if (cycle == time) then
        return true
    end
    return false
end

function ResourceTriggerCheck(resourceType, resourceAmount)
    if(resourceType == "Gold") then
        gold = GetGold()
        if (gold >= resourceAmount) then
            return true
        end
    elseif (resourceType == "Lumber") then
        lumber = GetLumber()
        if (lumber >= resourceAmount) then
            return true
        end
    end
    return false
end

function LocationTriggerCheck(x, y)
    Assets = GetAssets()
    if(not Assets) then
        return false
    end
    if (next(Assets) == nil) then
        return false
    end

    for _, v in pairs(Assets) do
        assetTilePosX, assetTilePosY = GetAssetTilePosition(v)
        if ((assetTilePosX == x) and (assetTilePosY) == y) then
            return true
        end
    end
end

function AssetObtainedTriggerCheck(assetType, assetAmount)
    if(GetPlayerAssetCount(assetType) >= assetAmount) then
        return true
    end
    return false
end

function AssetLostTriggerCheck(assetType, assetAmount)
    return HasLostUnits(assetType, assetAmount)
end
