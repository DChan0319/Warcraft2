-- helpers

--[[
-- Give the player stoneAmount stone
function GiveStoneEvent(stoneAmount)
    stone = GetStone()
    SetStone(stone + stoneAmount)
end
]]--

function DamageAssetEvent(tx, ty, damageAmount)
    Assets = GetAssets()
    if(next(Assets) == nil) then
        return 0
    end
    for _, v in pairs(Assets) do
        tilePosOfAssetX, tilePosOfAssetY = GetAssetTilePosition(v)
        if((tx == tilePosOfAssetX) and (ty == tilePosOfAssetY)) then
            DamageAsset(v, damageAmount)
            break
        end
    end
end

function HealAssetEvent(tx, ty, healAmount)
    Assets = GetAssets()
    if(next(Assets) == nil) then
        return 0
    end
    for _, v in pairs(Assets) do
        tilePosOfAssetX, tilePosOfAssetY = GetAssetTilePosition(v)
        if((tx == tilePosOfAssetX) and (ty == tilePosOfAssetY)) then
            hp = GetAssetHealth(v)
            SetAssetHealth(v, healAmount)
            break
        end
    end
end

-- Give the player goldAmount gold
function GiveGoldEvent(goldAmount)
    gold = GetGold()
    if (gold + goldAmount < 0) then
        SetGold(0)
    else
        SetGold(gold + goldAmount)
    end
end

-- Give the player lumberAmount lumber
function GiveLumberEvent(lumberAmount)
    lumber = GetLumber()
    if (lumber + lumberAmount < 0) then
        SetLumber(0)
    else
        SetLumber(lumber + lumberAmount)
    end
end

-- Create assetAmount of units of type assetType at (tx, ty)
function AddUnitsEvent(tx, ty, assetType, assetAmount)
    for i=1, assetAmount, 1 do
        AddAsset(assetType, tx, ty, 2)
    end
end

-- Create a building of type buildingType at (tx, ty)
function AddBuildingsEvent(tx, ty, buildingType, buildingAmount)
    for i=1, buildingAmount, 1 do 
        AddAsset(buildingType, tx, ty, 3)
    end
end
    
-- Give the player an instant win 
function InstaWinEvent()
    -- auto win
end

-- Give the player an instant loss
function InstaLossEvent()
    -- auto loss
end

