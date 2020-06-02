# Selfrole
Command for adding and removing self assignable roles, as well as showing a menu to assign roles with reactions.

## add
Add a role to a self role list. Optionally supply a list name at the end to put the role on a specific list.

```
r!selfrole add :two_hearts: @Yuri

r!selfrole add :two_hearts: @Yuri Anime
```

## list
Creates a reaction menu for the default or a custom self role list. Place this in an easy to access place as running the command again will invalidate old self role menus.

```
r!selfrole list

r!selfrole list Anime
```

> list create

Adds a new self role list. Names are case sensitive.
```
r!selfrole list create Anime
```
> list delete

Removes a self role list. Names are case sensitive. If the default self role list becomes corrupted, "default" can be provided to purge it.
```
r!selfrole list delete Anime
```

## remove
Removes a role from a self role list. Optionally supply a list name at the end to remove a role from a specific list.
```
r!selfrole remove :two_hearts:

r!selfrole remove :two_hearts: Anime
```

## Special Note:
Reimu will accept any standard, custom, or animated emote for role reactions, however it is recommended to only use standard or emotes from your server. If Reimu can not post the emote or looses access to it for any reason, menus may break or become corrupted requiring deleting the entire menu from the database.