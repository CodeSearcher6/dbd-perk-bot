using DBDPerkBot.Models;
using DBDPerkBot.Services;
using SixLabors.Fonts;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;
using SixLabors.ImageSharp.Drawing.Processing;
using System.Numerics;
using Img = SixLabors.ImageSharp.Image;
using SixColor = SixLabors.ImageSharp.Color;

namespace DBDPerkBot;

public class ImageComposer
{
    private readonly PerkStore _store;
    private readonly string _missingLog;
    private readonly Font _fontLabel;
    private readonly Font _fontTitle;

    public ImageComposer(PerkStore store)
    {
        _store = store;
        _missingLog = Path.Combine(AppContext.BaseDirectory, "missing_icons.txt");

        var fc = new FontCollection();
        try { fc.AddSystemFonts(); } catch { }

        var localFontPath = Path.Combine(AppContext.BaseDirectory, "assets", "Fonts", "Impact.ttf");
        if (File.Exists(localFontPath))
            fc.Add(localFontPath);

        var fam = fc.Families.Any()
            ? fc.Families.First()
            : SystemFonts.Collection.Families.First();

        _fontLabel = fam.CreateFont(22, FontStyle.Bold);
        _fontTitle = fam.CreateFont(52, FontStyle.Bold);

    }

    public async Task<Stream> ComposeAsync(List<Perk> perks, string mode)
    {
        const int card = 256;
        const int gap = 40;
        const int pad = 24;

        int width = perks.Count * card + (perks.Count - 1) * gap + pad * 2;
        int height = card + pad * 2 + 90;

        using var canvas = new Image<Rgba32>(width, height, new Rgba32(10, 10, 10));

        string title = ModeTitles.Get(mode);

        DrawTitle(canvas, title);
        ApplyVignette(canvas);

        int x = pad;
        int y = pad + 55;

        for (int i = 0; i < perks.Count; i++)
        {
            using var cardImg = await RenderPerkCardAsync(perks[i], card, card);
            canvas.Mutate(c => c.DrawImage(cardImg, new Point(x, y), 1f));

            string text = $"{i + 1}. {perks[i].Name}";

            canvas.Mutate(c => c.DrawText(
                new TextOptions(_fontLabel)
                {
                    Origin = new Vector2(x + card / 2f, y + card + 12),
                    HorizontalAlignment = HorizontalAlignment.Center
                },
                text,
                SixColor.White
            ));

            x += card + gap;
        }

        var ms = new MemoryStream();
        await canvas.SaveAsPngAsync(ms);
        await ms.FlushAsync();
        ms.Position = 0;
        return ms;

    }

    private void DrawTitle(Image<Rgba32> img, string text)
    {
        img.Mutate(ctx =>
        {
            ctx.Fill(new Rgba32(45, 0, 80, 220), new Rectangle(0, 0, img.Width, 60));
            var center = new PointF(img.Width / 2f, 5);

            for (int i = 0; i < 6; i++)
            {
                ctx.DrawText(
                    new TextOptions(_fontTitle)
                    {
                        Origin = new Vector2(center.X + i - 2, center.Y + i - 2),
                        HorizontalAlignment = HorizontalAlignment.Center
                    },
                    text,
                    SixColor.FromRgba(180, 100, 255, (byte)(90 - i * 12))
                );
            }

            ctx.DrawText(
                new TextOptions(_fontTitle)
                {
                    Origin = center,
                    HorizontalAlignment = HorizontalAlignment.Center
                },
                text,
                SixColor.ParseHex("#C77BFF")
            );
        });
    }

    private void ApplyVignette(Image<Rgba32> img)
    {
        int w = img.Width, h = img.Height;

        img.ProcessPixelRows(rows =>
        {
            var center = new Vector2(w / 2f, h / 2f);
            float maxDist = (float)Math.Sqrt(center.X * center.X + center.Y * center.Y);

            for (int y = 0; y < h; y++)
            {
                Span<Rgba32> row = rows.GetRowSpan(y);
                for (int x = 0; x < w; x++)
                {
                    float d = Math.Abs(Vector2.Distance(center, new Vector2(x, y)) / maxDist);
                    float v = Math.Min(d * 1.4f, 1f);
                    byte dark = (byte)(v * 140);

                    row[x].R = (byte)Math.Max(row[x].R - dark, 0);
                    row[x].G = (byte)Math.Max(row[x].G - dark, 0);
                    row[x].B = (byte)Math.Max(row[x].B - dark, 0);
                }
            }
        });
    }

    private async Task<Image<Rgba32>> RenderPerkCardAsync(Perk perk, int w, int h)
    {
        var card = new Image<Rgba32>(w, h, new Rgba32(25, 25, 25));

        var path = _store.ResolveIcon(perk.IconPath);
        Image<Rgba32>? icon;

        if (path != null && File.Exists(path))
            icon = await Img.LoadAsync<Rgba32>(path);
        else
        {
            try { await File.AppendAllTextAsync(_missingLog, $"{DateTime.Now} | {perk.IconPath} ({perk.Name})\n"); } catch { }
            icon = MakePlaceholderIcon(224, 224);
        }

        const int max = 224;
        float ratio = Math.Min((float)max / icon.Width, (float)max / icon.Height);
        int newW = (int)(icon.Width * ratio);
        int newH = (int)(icon.Height * ratio);

        int offsetX = (w - newW) / 2;
        int offsetY = (h - newH) / 2 + 4;

        icon.Mutate(i => i.Resize(newW, newH));
        card.Mutate(c => c.DrawImage(icon, new Point(offsetX, offsetY), 1f));

        for (int i = 0; i < 6; i++)
        {
            card.Mutate(c => c.Draw(SixColor.FromRgba(180, 80, 255, (byte)(120 - i * 18)), 2.5f,
                new Rectangle(i, i, w - 2 * i - 1, h - 2 * i - 1)));
        }

        for (int i = 0; i < 4; i++)
        {
            card.Mutate(c => c.Draw(SixColor.FromRgba(255, 210, 80, (byte)(90 - i * 20)), 1.8f,
                new Rectangle(i + 2, i + 2, w - (i + 2) * 2 - 1, h - (i + 2) * 2 - 1)));
        }

        card.Mutate(c => c.Draw(SixColor.FromRgb(160, 0, 255), 4, new Rectangle(0, 0, w - 1, h - 1)));

        icon.Dispose();
        return card;
    }

    private void ApplyFilmGrain(Image<Rgba32> img)
    {
        Random rnd = new Random();

        img.ProcessPixelRows(rows =>
        {
            for (int y = 0; y < img.Height; y++)
            {
                Span<Rgba32> row = rows.GetRowSpan(y);
                for (int x = 0; x < row.Length; x++)
                {
                    int n = rnd.Next(-5, 6);
                    row[x].R = (byte)Math.Max(0, Math.Min(255, row[x].R + n));
                    row[x].G = (byte)Math.Max(0, Math.Min(255, row[x].G + n));
                    row[x].B = (byte)Math.Max(0, Math.Min(255, row[x].B + n));
                }
            }
        });
    }

    private static Image<Rgba32> MakePlaceholderIcon(int w, int h)
    {
        var img = new Image<Rgba32>(w, h, new Rgba32(40, 40, 40));
        img.Mutate(c => c.Draw(SixColor.White, 3, new Rectangle(0, 0, w - 1, h - 1)));
        return img;
    }
}
